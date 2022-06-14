using Plus.Communication.Packets.Outgoing.Rooms.Furni.LoveLocks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.Drawing;

namespace Plus.HabboHotel.Items.Interactor;

public class InteractorLoveLock : IFurniInteractor
{
    public void OnPlace(GameClient session, Item item) { }

    public void OnRemove(GameClient session, Item item) { }

    public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
    {
        RoomUser user = null;
        if (session != null)
            user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return;
        if (Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y))
        {
            if (item.ExtraData == null || item.ExtraData.Length <= 1 || !item.ExtraData.Contains(Convert.ToChar(5).ToString()))
            {
                Point pointOne;
                Point pointTwo;
                switch (item.Rotation)
                {
                    case 2:
                        pointOne = new Point(item.GetX, item.GetY + 1);
                        pointTwo = new Point(item.GetX, item.GetY - 1);
                        break;
                    case 4:
                        pointOne = new Point(item.GetX - 1, item.GetY);
                        pointTwo = new Point(item.GetX + 1, item.GetY);
                        break;
                    default:
                        return;
                }
                var userOne = item.GetRoom().GetRoomUserManager().GetUserForSquare(pointOne.X, pointOne.Y);
                var userTwo = item.GetRoom().GetRoomUserManager().GetUserForSquare(pointTwo.X, pointTwo.Y);
                if (userOne == null || userTwo == null)
                    session.SendNotification("We couldn't find a valid user to lock this love lock with.");
                else if (userOne.GetClient() == null || userTwo.GetClient() == null)
                    session.SendNotification("We couldn't find a valid user to lock this love lock with.");
                else if (userOne.HabboId != item.UserId && userTwo.HabboId != item.UserId)
                    session.SendNotification("You can only use this item with the item owner.");
                else
                {
                    userOne.CanWalk = false;
                    userTwo.CanWalk = false;
                    item.InteractingUser = userOne.GetClient().GetHabbo().Id;
                    item.InteractingUser2 = userTwo.GetClient().GetHabbo().Id;
                    userOne.GetClient().SendPacket(new LoveLockDialogueMessageComposer(item.Id));
                    userTwo.GetClient().SendPacket(new LoveLockDialogueMessageComposer(item.Id));
                }
            }
            else
                return;
        }
        else
            user.MoveTo(item.SquareInFront);
    }

    public void OnWiredTrigger(Item item) { }
}