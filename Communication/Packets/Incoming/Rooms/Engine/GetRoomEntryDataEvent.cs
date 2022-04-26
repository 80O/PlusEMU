using System;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Quests;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine;

internal class GetRoomEntryDataEvent : IPacketEvent
{
    private readonly IQuestManager _questManager;

    public GetRoomEntryDataEvent(IQuestManager questManager)
    {
        _questManager = questManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        if (!room.GetRoomUserManager().AddAvatarToRoom(session))
        {
            room.GetRoomUserManager().RemoveUserFromRoom(session, false);
            return Task.CompletedTask; //TODO: Remove?
        }
        room.SendObjects(session);
        if (session.GetHabbo().GetMessenger() != null)
            session.GetHabbo().GetMessenger().OnStatusChanged(true);
        if (session.GetHabbo().GetStats().QuestId > 0)
            _questManager.QuestReminder(session, session.GetHabbo().GetStats().QuestId);
        session.SendPacket(new RoomEntryInfoComposer(room.RoomId, room.CheckRights(session, true)));
        session.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, Convert.ToBoolean(room.Hidewall)));
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
        if (user != null && session.GetHabbo().PetId == 0) room.SendPacket(new UserChangeComposer(user, false));
        session.SendPacket(new RoomEventComposer(room, room.Promotion));
        if (room.GetWired() != null)
            room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, session.GetHabbo());
        if (UnixTimestamp.GetNow() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
            session.SendPacket(new FloodControlComposer((int)session.GetHabbo().FloodTime - (int)UnixTimestamp.GetNow()));
        return Task.CompletedTask;
    }
}