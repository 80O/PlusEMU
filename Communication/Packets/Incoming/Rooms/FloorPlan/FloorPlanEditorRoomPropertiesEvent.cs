using System;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.FloorPlan;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.FloorPlan;

internal class FloorPlanEditorRoomPropertiesEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
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
        session.SendPacket(new FloorPlanFloorMapComposer(floorItems));
        session.SendPacket(new FloorPlanSendDoorComposer(model.DoorX, model.DoorY, model.DoorOrientation));
        session.SendPacket(new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, Convert.ToBoolean(room.Hidewall)));
        return Task.CompletedTask;
    }
}