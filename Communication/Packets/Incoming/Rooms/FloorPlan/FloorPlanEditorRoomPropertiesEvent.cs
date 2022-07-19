﻿using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.FloorPlan;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.FloorPlan;

internal class FloorPlanEditorRoomPropertiesEvent : IPacketEvent
{
    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var model = room.GetGameMap().Model;
        if (model == null)
            return Task.CompletedTask;
        var floorItems = room.GetRoomItemHandler().GetFloor;
        session.Send(new FloorPlanFloorMapComposer(floorItems));
        session.Send(new FloorPlanSendDoorComposer(model.DoorX, model.DoorY, model.DoorOrientation));
        session.Send(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, Convert.ToBoolean(room.Hidewall)));
        return Task.CompletedTask;
    }
}