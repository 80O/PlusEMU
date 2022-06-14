using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Wired;
using System.Text;

namespace Plus.HabboHotel.Rooms.Chat.Commands;

public class CommandManager : ICommandManager
{
    /// <summary>
    /// Commands registered for use.
    /// </summary>
    private readonly Dictionary<string, IChatCommand> _commands;
    /// <summary>
    /// Command Prefix only applies to custom commands.
    /// </summary>
    private readonly string _prefix = ":";

    /// <summary>
    /// The default initializer for the CommandManager
    /// </summary>
    public CommandManager(IEnumerable<IChatCommand> commands)
    {
        _commands = commands.ToDictionary(command => command.Key);
    }

    /// <summary>
    /// Request the text to parse and check for commands that need to be executed.
    /// </summary>
    /// <param name="session">Session calling this method.</param>
    /// <param name="message">The message to parse.</param>
    /// <returns>True if parsed or false if not.</returns>
    public bool Parse(GameClient session, string message)
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
            session.SendPacket(new MotdNotificationComposer(list.ToString()));
            return true;
        }
        message = message.Substring(1);
        var split = message.Split(' ');
        if (split.Length == 0)
            return false;
        IChatCommand cmd = null;
        if (_commands.TryGetValue(split[0].ToLower(), out cmd))
        {
            if (session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                LogCommand(session.GetHabbo().Id, message, session.GetHabbo().MachineId);
            if (!string.IsNullOrEmpty(cmd.PermissionRequired))
            {
                if (!session.GetHabbo().GetPermissions().HasCommand(cmd.PermissionRequired))
                    return false;
            }
            session.GetHabbo().ChatCommand = cmd;
            session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, session.GetHabbo(), this);
            cmd.Execute(session, session.GetHabbo().CurrentRoom, split);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Registers a Chat Command.
    /// </summary>
    /// <param name="commandText">Text to type for this command.</param>
    /// <param name="command">The command to execute.</param>
    public void Register(string commandText, IChatCommand command)
    {
        _commands.Add(commandText, command);
    }

    public static string MergeParams(string[] @params, int start)
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
        dbClient.AddParameter("MachineId", machineId);
        dbClient.AddParameter("Timestamp", PlusEnvironment.GetUnixTimestamp());
        dbClient.RunQuery();
    }

    public bool TryGetCommand(string command, out IChatCommand chatCommand) => _commands.TryGetValue(command, out chatCommand);
}