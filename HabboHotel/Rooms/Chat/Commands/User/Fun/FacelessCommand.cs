using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Users.Clothing;
using Plus.Database;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;

internal class FacelessCommand : IChatCommand
{
    private readonly IFigureDataManager _figureDataManager;
    private readonly IDatabase _database;
    public string Key => "faceless";
    public string PermissionRequired => "command_faceless";

    public string Parameters => "";

    public string Description => "Allows you to go faceless!";

    public FacelessCommand(IFigureDataManager figureDataManager, IDatabase database)
    {
        _figureDataManager = figureDataManager;
        _database = database;
    }

    public void Execute(GameClient session, Room room, string[] parameters)
    {
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null || user.GetClient() == null)
            return;
        string[] headParts;
        var figureParts = session.GetHabbo().Look.Split('.');
        foreach (var part in figureParts)
        {
            if (part.StartsWith("hd"))
            {
                headParts = part.Split('-');
                if (!headParts[1].Equals("99999"))//TODO add it to item list since its not valid yet.
                    headParts[1] = "99999";
                else
                    return;
                session.GetHabbo().Look = session.GetHabbo().Look.Replace(part, $"hd-{headParts[1]}-{headParts[2]}");
                break;
            }
        }
        session.GetHabbo().Look = _figureDataManager.ValidateLookAsync(session.GetHabbo().Look, session.GetHabbo().Gender, session.GetHabbo().Clothing.GetClothingParts, true).Result;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery($"UPDATE `users` SET `look` = '{session.GetHabbo().Look}' WHERE `id` = '{session.GetHabbo().Id}' LIMIT 1");
        }
        session.Send(new UserChangeComposer(user, true));
        session.GetHabbo().CurrentRoom.SendPacket(new UserChangeComposer(user, false));
    }
}