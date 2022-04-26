using System;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action;

internal class GiveHandItemEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IQuestManager _questManager;

    public GiveHandItemEvent(IRoomManager roomManager, IQuestManager questManager)
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
        var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(packet.PopInt());
        if (targetUser == null)
            return Task.CompletedTask;
        if (!(Math.Abs(user.X - targetUser.X) >= 3 || Math.Abs(user.Y - targetUser.Y) >= 3) || session.GetHabbo().GetPermissions().HasRight("mod_tool"))
        {
            if (user.CarryItemId > 0 && user.CarryTimer > 0)
            {
                if (user.CarryItemId == 8)
                    _questManager.ProgressUserQuest(session, QuestType.GiveCoffee);
                targetUser.CarryItem(user.CarryItemId);
                user.CarryItem(0);
                targetUser.DanceId = 0;
            }
        }
        return Task.CompletedTask;
    }
}