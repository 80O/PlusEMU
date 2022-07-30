using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands
{
    public interface ITargetChatCommand : ICommandBase
    {
        bool MustBeInSameRoom { get; }
        Task Execute(GameClient session, Room room, Habbo target, string[] parameters);
    }
}