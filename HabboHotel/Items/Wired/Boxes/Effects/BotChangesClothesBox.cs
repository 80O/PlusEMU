using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.Rooms;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects;

internal class BotChangesClothesBox : IWiredItem
{
    public BotChangesClothesBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.EffectBotChangesClothesBox;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        var unknown = packet.PopInt();
        var botConfiguration = packet.PopString();
        if (SetItems.Count > 0)
            SetItems.Clear();
        StringData = botConfiguration;
    }

    public bool Execute(params object[] @params)
    {
        if (@params == null || @params.Length == 0)
            return false;
        if (string.IsNullOrEmpty(StringData))
            return false;
        var stuff = StringData.Split('\t');
        if (stuff.Length != 2)
            return false; //This is important, incase a cunt scripts.
        var username = stuff[0];
        var user = Instance.GetRoomUserManager().GetBotByName(username);
        if (user == null)
            return false;
        var figure = stuff[1];
        var userChangeComposer = new ServerPacket(ServerPacketHeader.UserChangeMessageComposer);
        userChangeComposer.WriteInteger(user.VirtualId);
        userChangeComposer.WriteString(figure);
        userChangeComposer.WriteString("M");
        userChangeComposer.WriteString(user.BotData.Motto);
        userChangeComposer.WriteInteger(0);
        Instance.SendPacket(userChangeComposer);
        user.BotData.Look = figure;
        user.BotData.Gender = "M";
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("UPDATE `bots` SET `look` = @look, `gender` = @gender WHERE `id` = '" + user.BotData.Id + "' LIMIT 1");
        dbClient.AddParameter("look", user.BotData.Look);
        dbClient.AddParameter("gender", user.BotData.Gender);
        dbClient.RunQuery();
        return true;
    }
}