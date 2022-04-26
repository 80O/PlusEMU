using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Users;

internal class RespectUserEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IQuestManager _questManager;

    public RespectUserEvent(IRoomManager roomManager, IAchievementManager achievementManager, IQuestManager questManager)
    {
        _roomManager = roomManager;
        _achievementManager = achievementManager;
        _questManager = questManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom || session.GetHabbo().GetStats().DailyRespectPoints <= 0)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(packet.PopInt());
        if (user == null || user.GetClient() == null || user.GetClient().GetHabbo().Id == session.GetHabbo().Id || user.IsBot)
            return Task.CompletedTask;
        var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (thisUser == null)
            return Task.CompletedTask;
        _questManager.ProgressUserQuest(session, QuestType.SocialRespect);
        _achievementManager.ProgressAchievement(session, "ACH_RespectGiven", 1);
        _achievementManager.ProgressAchievement(user.GetClient(), "ACH_RespectEarned", 1);
        session.GetHabbo().GetStats().DailyRespectPoints -= 1;
        session.GetHabbo().GetStats().RespectGiven += 1;
        user.GetClient().GetHabbo().GetStats().Respect += 1;
        if (room.RespectNotificationsEnabled)
            room.SendPacket(new RespectNotificationComposer(user.GetClient().GetHabbo().Id, user.GetClient().GetHabbo().GetStats().Respect));
        room.SendPacket(new ActionComposer(thisUser.VirtualId, 7));
        return Task.CompletedTask;
    }
}