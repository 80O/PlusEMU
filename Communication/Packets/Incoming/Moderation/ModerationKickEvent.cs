using System.Threading.Tasks;
using Plus.Core.Language;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Moderation;

internal class ModerationKickEvent : IPacketEvent
{
    private readonly IGameClientManager _clientManager;
    private readonly ILanguageManager _languageManager;
    private readonly IRoomManager _roomManger;

    public ModerationKickEvent(IGameClientManager clientManager, ILanguageManager languageManager, IRoomManager roomManager)
    {
        _clientManager = clientManager;
        _languageManager = languageManager;
        _roomManger = roomManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().GetPermissions().HasRight("mod_kick"))
            return Task.CompletedTask;
        var userId = packet.PopInt();
        packet.PopString(); //message
        var client = _clientManager.GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null || client.GetHabbo().CurrentRoomId < 1 || client.GetHabbo().Id == session.GetHabbo().Id)
            return Task.CompletedTask;
        if (client.GetHabbo().Rank >= session.GetHabbo().Rank)
        {
            session.SendNotification(_languageManager.TryGetValue("moderation.kick.disallowed"));
            return Task.CompletedTask;
        }
        if (!_roomManger.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        room.GetRoomUserManager().RemoveUserFromRoom(client, true);
        return Task.CompletedTask;
    }
}