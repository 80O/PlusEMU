using System;
using System.Threading.Tasks;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class BanUserEvent : IPacketEvent
{
    private readonly IAchievementManager _achievementManager;

    public BanUserEvent(IAchievementManager achievementManager)
    {
        _achievementManager = achievementManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        if (room.WhoCanBan == 0 && !room.CheckRights(session, true) && room.Group == null || room.WhoCanBan == 1 && !room.CheckRights(session) && room.Group == null ||
            room.Group != null && !room.CheckRights(session, false, true))
            return Task.CompletedTask;
        var userId = packet.PopInt();
        packet.PopInt(); //roomId
        var r = packet.PopString();
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToInt32(userId));
        if (user == null || user.IsBot)
            return Task.CompletedTask;
        if (room.OwnerId == userId)
            return Task.CompletedTask;
        if (user.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
            return Task.CompletedTask;
        long time = 0;
        if (r.ToLower().Contains("hour"))
            time = 3600;
        else if (r.ToLower().Contains("day"))
            time = 86400;
        else if (r.ToLower().Contains("perm"))
            time = 78892200;
        room.GetBans().Ban(user, time);
        _achievementManager.ProgressAchievement(session, "ACH_SelfModBanSeen", 1);
        return Task.CompletedTask;
    }
}