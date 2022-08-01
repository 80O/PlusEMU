using System.Collections.Concurrent;
using System.Text;
using NLog.Targets;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Wired;

namespace Plus.HabboHotel.Rooms.Chat.Commands;

public class CommandManager : ICommandManager
{
    private readonly IGameClientManager _gameClientManager;
    /// <summary>
    /// Commands registered for use.
    /// </summary>
    private readonly ConcurrentDictionary<string, ICommandBase> _commands;
    /// <summary>
    /// Command Prefix only applies to custom commands.
    /// </summary>
    private readonly string _prefix = ":";

    /// <summary>
    /// The default initializer for the CommandManager
    /// </summary>
    public CommandManager(IEnumerable<ICommandBase> commands, IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
        _commands = new(commands.ToDictionary(command => command.Key));
    }

    /// <summary>
    /// Request the text to parse and check for commands that need to be executed.
    /// </summary>
    /// <param name="session">Session calling this method.</param>
    /// <param name="message">The message to parse.</param>
    /// <returns>True if parsed or false if not.</returns>
    public async Task<bool> Parse(GameClient session, string message)
    {
        if (session == null || session.GetHabbo() == null || session.GetHabbo().CurrentRoom == null)
            return false;
        if (!message.StartsWith(_prefix))
            return false;
        if (message == _prefix + "commands")
        {
            var list = new StringBuilder();
            list.Append("This is the list of commands you have available:\n");
            foreach (var cmdList in _commands.ToList())
            {
                if (!string.IsNullOrEmpty(cmdList.Value.PermissionRequired))
                {
                    if (!session.GetHabbo().GetPermissions().HasCommand(cmdList.Value.PermissionRequired))
                        continue;
                }
                list.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
            }
            session.Send(new MotdNotificationComposer(list.ToString()));
            return true;
        }
        message = message.Substring(1);
        if (string.IsNullOrWhiteSpace(message))
            return false;

        var split = message.Split(' ');
        var key = split[0];
        var parameters = split.Length > 1 ? split[1..] : Array.Empty<string>();
        if (_commands.TryGetValue(key.ToLower(), out var command))
        {
            if (session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                LogCommand(session.GetHabbo().Id, message, session.GetHabbo().MachineId);
            if (!string.IsNullOrEmpty(command.PermissionRequired))
            {
                if (!session.GetHabbo().GetPermissions().HasCommand(command.PermissionRequired))
                    return false;
            }
            session.GetHabbo().ChatCommand = command;
            session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, session.GetHabbo(), this);

            if (command is IChatCommand chatCommand)
            {
                chatCommand.Execute(session, session.GetHabbo().CurrentRoom, parameters);
            }
            else if (command is ITargetChatCommand targetChatCommand)
            {
                if (!parameters.Any())
                {
                    session.SendWhisper("No username specified.");
                    return true;
                }

                var username = parameters[0];
                parameters = parameters.Length > 1 ? parameters[1..] : Array.Empty<string>();
                var target = _gameClientManager.GetClientByUsername(username);
                if (target == null)
                {
                    session.SendWhisper($"User {username} seems to be offline.");
                    return true;
                }

                if (targetChatCommand.MustBeInSameRoom && session.GetHabbo().CurrentRoomId != target.GetHabbo().CurrentRoomId)
                {
                    session.SendWhisper($"You must be in the same room as {username} to execute this command.");
                    return true;
                }

                await targetChatCommand.Execute(session, session.GetHabbo().CurrentRoom, target.GetHabbo(), parameters);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Registers a Chat Command.
    /// </summary>
    /// <param name="commandText">Text to type for this command.</param>
    /// <param name="command">The command to execute.</param>
    public void Register(string commandText, ICommandBase command)
    {
        _commands.TryAdd(commandText, command);
    }

    public static string MergeParams(string[] @params, int start = 0)
    {
        var merged = new StringBuilder();
        for (var i = start; i < @params.Length; i++)
        {
            if (i > start)
                merged.Append(" ");
            merged.Append(@params[i]);
        }
        return merged.ToString();
    }

    public void LogCommand(int userId, string data, string machineId)
    {
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.SetQuery("INSERT INTO `logs_client_staff` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
        dbClient.AddParameter("UserId", userId);
        dbClient.AddParameter("Data", data);
        dbClient.AddParameter("MachineId", machineId ?? string.Empty);
        dbClient.AddParameter("Timestamp", PlusEnvironment.GetUnixTimestamp());
        dbClient.RunQuery();
    }

    public bool TryGetCommand(string command, out ICommandBase chatCommand) => _commands.TryGetValue(command, out chatCommand);
}