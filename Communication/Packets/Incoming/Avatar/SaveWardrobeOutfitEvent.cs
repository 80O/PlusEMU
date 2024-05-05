using Plus.HabboHotel.Users.Clothing;
using Plus.Database;
using Plus.HabboHotel.GameClients;

using Dapper;

namespace Plus.Communication.Packets.Incoming.Avatar;

internal class SaveWardrobeOutfitEvent : IPacketEvent
{
    private readonly IFigureDataManager _figureDataManager;
    private readonly IDatabase _database;

    public SaveWardrobeOutfitEvent(IFigureDataManager figureDataManager, IDatabase database)
    {
        _figureDataManager = figureDataManager;
        _database = database;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var slotId = packet.ReadInt();
        var look = packet.ReadString();
        var gender = packet.ReadString();
        look = _figureDataManager.ValidateLookAsync(look, gender, session.GetHabbo().Clothing.GetClothingParts, true).Result;

        using (var connection = _database.Connection())
        {
            int rows = connection.Execute("SELECT null FROM `user_wardrobe` WHERE `user_id` = @id AND `slot_id` = @slot",
                new { id = session.GetHabbo().Id, slot = slotId });

            if (rows == 1)
            {
                connection.Execute("UPDATE `user_wardrobe` SET `look` = @look, `gender` = @gender WHERE `user_id` = @id AND `slot_id` = @slot LIMIT 1",
                    new { look = look, gender = gender.ToUpper(), id = session.GetHabbo().Id, slot = slotId });
            }
            else
            {
                connection.Execute("INSERT INTO `user_wardrobe` (`user_id`,`slot_id`,`look`,`gender`) VALUES (@id,@slot,@look,@gender)",
                    new { id = session.GetHabbo().Id, slot = slotId, look, gender = gender.ToUpper() });
            }
        }
        return Task.CompletedTask;
    }
}