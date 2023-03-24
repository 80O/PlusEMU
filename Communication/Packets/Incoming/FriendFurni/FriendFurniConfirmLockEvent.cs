using Plus.Communication.Packets.Outgoing.Rooms.Furni.LoveLocks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.FriendFurni;

internal class FriendFurniConfirmLockEvent : IPacketEvent
{
    private readonly IItemDataManager _itemDataManager;

    public FriendFurniConfirmLockEvent(IItemDataManager itemDataManager) =>
        _itemDataManager = itemDataManager;

    public async Task Parse(GameClient session, IIncomingPacket packet)
    {
        var pId = packet.ReadUInt();
        var isConfirmed = packet.ReadBool();
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return;
        var item = room.GetRoomItemHandler().GetItem(pId);
        if (item == null || item.Definition == null || item.Definition.InteractionType != InteractionType.Lovelock)
            return;
        var userOneId = item.InteractingUser;
        var userTwoId = item.InteractingUser2;
        var userOne = room.GetRoomUserManager().GetRoomUserByHabbo(userOneId);
        var userTwo = room.GetRoomUserManager().GetRoomUserByHabbo(userTwoId);
        if (userOne == null && userTwo == null)
        {
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            session.SendNotification("Your partner has left the room or has cancelled the love lock.");
            return;
        }
        if (userOne.GetClient() == null || userTwo.GetClient() == null)
        {
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            session.SendNotification("Your partner has left the room or has cancelled the love lock.");
            return;
        }
        if (userOne == null)
        {
            userTwo.CanWalk = true;
            userTwo.GetClient().SendNotification("Your partner has left the room or has cancelled the love lock.");
            userTwo.LlPartner = 0;
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            return;
        }
        if (userTwo == null)
        {
            userOne.CanWalk = true;
            userOne.GetClient().SendNotification("Your partner has left the room or has cancelled the love lock.");
            userOne.LlPartner = 0;
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            return;
        }
        if (item.ExtraData.Serialize().Contains(Convert.ToChar(5).ToString()))
        {
            userTwo.CanWalk = true;
            userTwo.GetClient().SendNotification("It appears this love lock has already been locked.");
            userTwo.LlPartner = 0;
            userOne.CanWalk = true;
            userOne.GetClient().SendNotification("It appears this love lock has already been locked.");
            userOne.LlPartner = 0;
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            return;
        }
        if (!isConfirmed)
        {
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            userOne.LlPartner = 0;
            userTwo.LlPartner = 0;
            userOne.CanWalk = true;
            userTwo.CanWalk = true;
            return;
        }
        if (userOneId == session.GetHabbo().Id)
        {
            session.Send(new LoveLockDialogueSetLockedComposer(pId));
            userOne.LlPartner = userTwoId;
        }
        else if (userTwoId == session.GetHabbo().Id)
        {
            session.Send(new LoveLockDialogueSetLockedComposer(pId));
            userTwo.LlPartner = userOneId;
        }
        if (userOne.LlPartner == 0 || userTwo.LlPartner == 0)
            return;
        item.ExtraData.Store(
            $"1{(char)5}{userOne.GetUsername()}{(char)5}{userTwo.GetUsername()}{(char)5}{userOne.GetClient().GetHabbo().Look}{(char)5}{userTwo.GetClient().GetHabbo().Look}{(char)5}{DateTime.Now:dd/MM/yyyy}");
        item.InteractingUser = 0;
        item.InteractingUser2 = 0;
        userOne.LlPartner = 0;
        userTwo.LlPartner = 0;
        item.UpdateState(true, true);
        await _itemDataManager.UpdateItemExtradata(item);
        userOne.GetClient().Send(new LoveLockDialogueCloseComposer(pId));
        userTwo.GetClient().Send(new LoveLockDialogueCloseComposer(pId));
        userOne.CanWalk = true;
        userTwo.CanWalk = true;
    }
}