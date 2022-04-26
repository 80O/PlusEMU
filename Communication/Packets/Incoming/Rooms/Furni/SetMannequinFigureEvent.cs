using System;
using System.Linq;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni;

internal class SetMannequinFigureEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var room = session.GetHabbo().CurrentRoom;
        if (room == null || !room.CheckRights(session, true))
            return Task.CompletedTask;
        var itemId = packet.PopInt();
        var item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
        if (item == null)
            return Task.CompletedTask;
        var gender = session.GetHabbo().Gender.ToLower();
        var figure = session.GetHabbo().Look.Split('.').Where(str => !str.Contains("hr") && !str.Contains("hd") && !str.Contains("he") && !str.Contains("ea") && !str.Contains("ha"))
            .Aggregate("", (current, str) => current + str + ".");
        figure = figure.TrimEnd('.');
        if (item.ExtraData.Contains(Convert.ToChar(5)))
        {
            var flags = item.ExtraData.Split(Convert.ToChar(5));
            item.ExtraData = gender + Convert.ToChar(5) + figure + Convert.ToChar(5) + flags[2];
        }
        else
            item.ExtraData = gender + Convert.ToChar(5) + figure + Convert.ToChar(5) + "Default";
        item.UpdateState(true, true);
        return Task.CompletedTask;
    }
}