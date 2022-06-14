using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.PathFinding;
using System.Drawing;

namespace Plus.HabboHotel.Items.Interactor;

public class InteractorPuzzleBox : IFurniInteractor
{
    public void OnPlace(GameClient session, Item item) { }

    public void OnRemove(GameClient session, Item item) { }

    public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
    {
        if (session == null)
            return;
        var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null) return;
        if (!(Math.Abs(user.X - item.GetX) >= 2 || Math.Abs(user.Y - item.GetY) >= 2))
        {
            user.SetRot(Rotation.Calculate(user.X, user.Y, item.GetX, item.GetY), false);
            if (user.RotBody % 2 != 0)
            {
                user.MoveTo(item.GetX + 1, item.GetY);
                return;
            }
            var room = item.GetRoom();
            var newPoint = new Point(0, 0);
            if (user.RotBody == 4) newPoint = new Point(item.GetX, item.GetY + 1);
            if (user.RotBody == 0) newPoint = new Point(item.GetX, item.GetY - 1);
            if (user.RotBody == 6) newPoint = new Point(item.GetX - 1, item.GetY);
            if (user.RotBody == 2) newPoint = new Point(item.GetX + 1, item.GetY);
            if (room.GetGameMap().ValidTile(newPoint.X, newPoint.Y) &&
                room.GetGameMap().ItemCanBePlaced(newPoint.X, newPoint.Y) &&
                room.GetGameMap().CanRollItemHere(newPoint.X, newPoint.Y))
            {
                var newZ = item.GetRoom().GetGameMap().SqAbsoluteHeight(newPoint.X, newPoint.Y);
                /*var mMessage = new ServerMessage();
                mMessage.Init(Outgoing.ObjectOnRoller); // Cf
                mMessage.AppendInt32(Item.GetX);
                mMessage.AppendInt32(Item.GetY);
                mMessage.AppendInt32(NewPoint.X);
                mMessage.AppendInt32(NewPoint.Y);
                mMessage.AppendInt32(1);
                mMessage.AppendInt32(Item.Id);
                mMessage.AppendString(Item.GetZ.ToString().Replace(',', '.'));
                mMessage.AppendString(NewZ.ToString().Replace(',', '.'));
                mMessage.AppendInt32(0);
                Room.SendMessage(mMessage);*/
                room.SendPacket(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, newPoint.X, newPoint.Y, newZ, 0, 0, item.Id));
                item.GetRoom().GetRoomItemHandler().SetFloorItem(user.GetClient(), item, newPoint.X, newPoint.Y, item.Rotation, false, false, false);
            }
        }
        else
            user.MoveTo(item.GetX + 1, item.GetY);
    }

    public void OnWiredTrigger(Item item) { }
}