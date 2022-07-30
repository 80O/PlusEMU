using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands;

public interface IChatCommand : ICommandBase
{
    void Execute(GameClient session, Room room, string[] parameters);
}