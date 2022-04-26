using System;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.LoveLocks;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.LoveLocks;

internal class ConfirmLoveLockEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public ConfirmLoveLockEvent(IDatabase database)
    {
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var pId = packet.PopInt();
        var isConfirmed = packet.PopBoolean();
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var item = room.GetRoomItemHandler().GetItem(pId);
        if (item == null || item.GetBaseItem() == null || item.GetBaseItem().InteractionType != InteractionType.Lovelock)
            return Task.CompletedTask;
        var userOneId = item.InteractingUser;
        var userTwoId = item.InteractingUser2;
        var userOne = room.GetRoomUserManager().GetRoomUserByHabbo(userOneId);
        var userTwo = room.GetRoomUserManager().GetRoomUserByHabbo(userTwoId);
        if (userOne == null && userTwo == null)
        {
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            session.SendNotification("Your partner has left the room or has cancelled the love lock.");
            return Task.CompletedTask;
        }
        if (userOne.GetClient() == null || userTwo.GetClient() == null)
        {
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            session.SendNotification("Your partner has left the room or has cancelled the love lock.");
            return Task.CompletedTask;
        }
        if (userOne == null)
        {
            userTwo.CanWalk = true;
            userTwo.GetClient().SendNotification("Your partner has left the room or has cancelled the love lock.");
            userTwo.LlPartner = 0;
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            return Task.CompletedTask;
        }
        if (userTwo == null)
        {
            userOne.CanWalk = true;
            userOne.GetClient().SendNotification("Your partner has left the room or has cancelled the love lock.");
            userOne.LlPartner = 0;
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            return Task.CompletedTask;
        }
        if (item.ExtraData.Contains(Convert.ToChar(5).ToString()))
        {
            userTwo.CanWalk = true;
            userTwo.GetClient().SendNotification("It appears this love lock has already been locked.");
            userTwo.LlPartner = 0;
            userOne.CanWalk = true;
            userOne.GetClient().SendNotification("It appears this love lock has already been locked.");
            userOne.LlPartner = 0;
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            return Task.CompletedTask;
        }
        if (!isConfirmed)
        {
            item.InteractingUser = 0;
            item.InteractingUser2 = 0;
            userOne.LlPartner = 0;
            userTwo.LlPartner = 0;
            userOne.CanWalk = true;
            userTwo.CanWalk = true;
            return Task.CompletedTask;
        }
        if (userOneId == session.GetHabbo().Id)
        {
            session.SendPacket(new LoveLockDialogueSetLockedMessageComposer(pId));
            userOne.LlPartner = userTwoId;
        }
        else if (userTwoId == session.GetHabbo().Id)
        {
            session.SendPacket(new LoveLockDialogueSetLockedMessageComposer(pId));
            userTwo.LlPartner = userOneId;
        }
        if (userOne.LlPartner == 0 || userTwo.LlPartner == 0)
            return Task.CompletedTask;
        item.ExtraData = "1" + (char)5 + userOne.GetUsername() + (char)5 + userTwo.GetUsername() + (char)5 + userOne.GetClient().GetHabbo().Look + (char)5 + userTwo.GetClient().GetHabbo().Look +
                         (char)5 + DateTime.Now.ToString("dd/MM/yyyy");
        item.InteractingUser = 0;
        item.InteractingUser2 = 0;
        userOne.LlPartner = 0;
        userTwo.LlPartner = 0;
        item.UpdateState(true, true);
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `items` SET `extra_data` = @extraData WHERE `id` = @ID LIMIT 1");
            dbClient.AddParameter("extraData", item.ExtraData);
            dbClient.AddParameter("ID", item.Id);
            dbClient.RunQuery();
        }
        userOne.GetClient().SendPacket(new LoveLockDialogueCloseMessageComposer(pId));
        userTwo.GetClient().SendPacket(new LoveLockDialogueCloseMessageComposer(pId));
        userOne.CanWalk = true;
        userTwo.CanWalk = true;
        userOne = null;
        userTwo = null;
        return Task.CompletedTask;
    }
}