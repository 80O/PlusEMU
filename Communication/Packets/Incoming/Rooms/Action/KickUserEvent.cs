using System.Threading.Tasks;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class KickUserEvent : IPacketEvent
{
    private readonly IAchievementManager _achievementManager;

    public KickUserEvent(IAchievementManager achievementManager)
    {
        _achievementManager = achievementManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        if (!room.CheckRights(session) && room.WhoCanKick != 2 && room.Group == null)
            return Task.CompletedTask;
        if (room.Group != null && !room.CheckRights(session, false, true))
            return Task.CompletedTask;
        var userId = packet.PopInt();
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
        if (user == null || user.IsBot)
            return Task.CompletedTask;

        //Cannot kick owner or moderators.
        if (room.CheckRights(user.GetClient(), true) || user.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
            return Task.CompletedTask;
        room.GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, true);
        _achievementManager.ProgressAchievement(session, "ACH_SelfModKickSeen", 1);
        return Task.CompletedTask;
    }
}