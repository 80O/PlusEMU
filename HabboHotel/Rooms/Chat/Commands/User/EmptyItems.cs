using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User;

internal class EmptyItems : IChatCommand
{
    public string Key => "emptyitems";
    public string PermissionRequired => "command_empty_items";

    public string Parameters => "%yes%";

    public string Description => "Is your inventory full? You can remove all items by typing this command.";

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        if (!parameters.Any())
        {
            session.SendNotification("Are you sure you want to clear your inventory? You will lose all the furniture!\n" +
                                     "To confirm, type \":emptyitems yes\". \n\nOnce you do this, there is no going back!\n(If you do not want to empty it, just ignore this message!)\n\n" +
                                     "PLEASE NOTE! If you have more than 3000 items, the hidden items will also be DELETED.");
            return;
        }
        if (parameters.Length == 1 && parameters[0] == "yes")
        {
            ItemLoader.DeleteAllInventoryItemsForUser(session.GetHabbo().Id);
            session.GetHabbo().Inventory.Furniture.ClearItems();
            session.Send(new FurniListUpdateComposer());
            session.SendNotification("Your inventory has been cleared!");
            return;
        }
        if (parameters.Length == 1 && parameters[0] != "yes") session.SendNotification("To confirm, you must type in :emptyitems yes");
    }
}