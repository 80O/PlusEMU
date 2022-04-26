using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar;

internal class DanceEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IQuestManager _questManager;

    public DanceEvent(IRoomManager roomManager, IQuestManager questManager)
    {
        _roomManager = roomManager;
        _questManager = questManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return Task.CompletedTask;
        user.UnIdle();
        var danceId = packet.PopInt();
        if (danceId < 0 || danceId > 4)
            danceId = 0;
        if (danceId > 0 && user.CarryItemId > 0)
            user.CarryItem(0);
        if (session.GetHabbo().Effects().CurrentEffect > 0)
            room.SendPacket(new AvatarEffectComposer(user.VirtualId, 0));
        user.DanceId = danceId;
        room.SendPacket(new DanceComposer(user, danceId));
        _questManager.ProgressUserQuest(session, QuestType.SocialDance);
        if (room.GetRoomUserManager().GetRoomUsers().Count > 19)
            _questManager.ProgressUserQuest(session, QuestType.MassDance);
        return Task.CompletedTask;
    }
}