using System.Threading.Tasks;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class MuteUserEvent : IPacketEvent
{
    private readonly IAchievementManager _achievementManager;

    public MuteUserEvent(IAchievementManager achievementManager)
    {
        _achievementManager = achievementManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var userId = packet.PopInt();
        packet.PopInt(); //roomId
        var time = packet.PopInt();
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        if (room.WhoCanMute == 0 && !room.CheckRights(session, true) && room.Group == null || room.WhoCanMute == 1 && !room.CheckRights(session) && room.Group == null ||
            room.Group != null && !room.CheckRights(session, false, true))
            return Task.CompletedTask;
        var target = room.GetRoomUserManager().GetRoomUserByHabbo(PlusEnvironment.GetUsernameById(userId));
        if (target == null)
            return Task.CompletedTask;
        if (target.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
            return Task.CompletedTask;
        if (room.MutedUsers.ContainsKey(userId))
        {
            if (room.MutedUsers[userId] < UnixTimestamp.GetNow())
                room.MutedUsers.Remove(userId);
            else
                return Task.CompletedTask;
        }
        room.MutedUsers.Add(userId, UnixTimestamp.GetNow() + time * 60);
        target.GetClient().SendWhisper("The room owner has muted you for " + time + " minutes!");
        _achievementManager.ProgressAchievement(session, "ACH_SelfModMuteSeen", 1);
        return Task.CompletedTask;
    }
}