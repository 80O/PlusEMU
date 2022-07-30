using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands;

public interface ICommandManager
{
    /// <summary>
    /// Request the text to parse and check for commands that need to be executed.
    /// </summary>
    /// <param name="session">Session calling this method.</param>
    /// <param name="message">The message to parse.</param>
    /// <returns>True if parsed or false if not.</returns>
    Task<bool> Parse(GameClient session, string message);

    /// <summary>
    /// Registers a Chat Command.
    /// </summary>
    /// <param name="commandText">Text to type for this command.</param>
    /// <param name="command">The command to execute.</param>
    void Register(string commandText, ICommandBase command);

    void LogCommand(int userId, string data, string machineId);
    bool TryGetCommand(string command, out ICommandBase chatCommand);
}