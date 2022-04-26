using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Users;

internal class GetHabboGroupBadgesEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;

    public GetHabboGroupBadgesEvent(IGroupManager groupManager)
    {
        _groupManager = groupManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var badges = new Dictionary<int, string>();
        foreach (var user in room.GetRoomUserManager().GetRoomUsers().ToList())
        {
            if (user.IsBot || user.IsPet || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                continue;
            if (user.GetClient().GetHabbo().GetStats().FavouriteGroupId == 0 || badges.ContainsKey(user.GetClient().GetHabbo().GetStats().FavouriteGroupId))
                continue;
            if (!_groupManager.TryGetGroup(user.GetClient().GetHabbo().GetStats().FavouriteGroupId, out var group))
                continue;
            if (!badges.ContainsKey(group.Id))
                badges.Add(group.Id, group.Badge);
        }
        if (session.GetHabbo().GetStats().FavouriteGroupId > 0)
        {
            if (_groupManager.TryGetGroup(session.GetHabbo().GetStats().FavouriteGroupId, out var group))
            {
                if (!badges.ContainsKey(group.Id))
                    badges.Add(group.Id, group.Badge);
            }
        }
        room.SendPacket(new HabboGroupBadgesComposer(badges));
        session.SendPacket(new HabboGroupBadgesComposer(badges));
        return Task.CompletedTask;
    }
}