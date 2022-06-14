using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Users;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects;

internal class RemoveActorFromTeamBox : IWiredItem
{
    public RemoveActorFromTeamBox(Room instance, Item item)
    {
        Instance = instance;
        Item = item;
        SetItems = new ConcurrentDictionary<int, Item>();
    }

    public Room Instance { get; set; }
    public Item Item { get; set; }
    public WiredBoxType Type => WiredBoxType.EffectRemoveActorFromTeam;
    public ConcurrentDictionary<int, Item> SetItems { get; set; }
    public string StringData { get; set; }
    public bool BoolData { get; set; }
    public string ItemsData { get; set; }

    public void HandleSave(ClientPacket packet)
    {
        var unknown = packet.PopInt();
    }

    public bool Execute(params object[] @params)
    {
        if (@params.Length == 0 || Instance == null)
            return false;
        var player = (Habbo)@params[0];
        if (player == null)
            return false;
        var user = Instance.GetRoomUserManager().GetRoomUserByHabbo(player.Id);
        if (user == null)
            return false;
        if (user.Team != Team.None)
        {
            var team = Instance.GetTeamManagerForFreeze();
            if (team != null)
            {
                team.OnUserLeave(user);
                user.Team = Team.None;
                if (user.GetClient().GetHabbo().Effects().CurrentEffect != 0)
                    user.GetClient().GetHabbo().Effects().ApplyEffect(0);
            }
        }
        return true;
    }
}