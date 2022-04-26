using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar;

public class ActionEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IQuestManager _questManager;

    public ActionEvent(IRoomManager roomManager, IQuestManager questManager)
    {
        _roomManager = roomManager;
        _questManager = questManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var action = packet.PopInt();
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return Task.CompletedTask;
        if (user.DanceId > 0)
            user.DanceId = 0;
        if (session.GetHabbo().Effects().CurrentEffect > 0)
            room.SendPacket(new AvatarEffectComposer(user.VirtualId, 0));
        user.UnIdle();
        room.SendPacket(new ActionComposer(user.VirtualId, action));
        if (action == 5) // idle
        {
            user.IsAsleep = true;
            room.SendPacket(new SleepComposer(user, true));
        }
        _questManager.ProgressUserQuest(session, QuestType.SocialWave);
        return Task.CompletedTask;
    }
}