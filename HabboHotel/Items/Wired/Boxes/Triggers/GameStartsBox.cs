using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Triggers;

internal class GameStartsBox : IWiredItem
{
    public GameStartsBox(Room instance, Item item)
    {
        Item = item;
        Instance = instance;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.TriggerGameStarts;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet) { }

    public bool Execute(params object[] @params)
    {
        var effects = Instance.GetWired().GetEffects(this);
        var conditions = Instance.GetWired().GetConditions(this);
        foreach (var condition in conditions) Instance.GetWired().OnEvent(condition.Item);

        //Check the ICollection to find the random addon effect.
        var hasRandomEffectAddon = effects.Count(x => x.Type == WiredBoxType.AddonRandomEffect) > 0;
        if (hasRandomEffectAddon)
        {
            //Okay, so we have a random addon effect, now lets get the IWiredItem and attempt to execute it.
            var randomBox = effects.FirstOrDefault(x => x.Type == WiredBoxType.AddonRandomEffect);
            if (!randomBox.Execute())
                return false;

            //Success! Let's get our selected box and continue.
            var selectedBox = Instance.GetWired().GetRandomEffect(effects.ToList());
            if (!selectedBox.Execute())
                return false;

            //Woo! Almost there captain, now lets broadcast the update to the room instance.
            if (Instance != null)
            {
                Instance.GetWired().OnEvent(randomBox.Item);
                Instance.GetWired().OnEvent(selectedBox.Item);
            }
        }
        else
        {
            foreach (var effect in effects)
            {
                foreach (var user in Instance.GetRoomUserManager().GetRoomUsers().ToList())
                {
                    if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                        continue;
                    effect.Execute(user.GetClient().GetHabbo());
                }
                Instance.GetWired().OnEvent(effect.Item);
            }
        }
        return true;
    }
}