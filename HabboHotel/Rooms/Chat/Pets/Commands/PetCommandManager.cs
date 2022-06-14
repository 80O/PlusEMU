using System.Data;

namespace Plus.HabboHotel.Rooms.Chat.Pets.Commands;

public class PetCommandManager : IPetCommandManager
{
    private readonly Dictionary<string, string> _commandDatabase;
    private readonly Dictionary<int, string> _commandRegister;
    private readonly Dictionary<string, PetCommand> _petCommands;

    public PetCommandManager()
    {
        _petCommands = new Dictionary<string, PetCommand>();
        _commandRegister = new Dictionary<int, string>();
        _commandDatabase = new Dictionary<string, string>();
    }

    public void Init()
    {
        _petCommands.Clear();
        _commandRegister.Clear();
        _commandDatabase.Clear();
        DataTable table = null;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT * FROM `bots_pet_commands`");
            table = dbClient.GetTable();
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    _commandRegister.Add(Convert.ToInt32(row[0]), row[1].ToString());
                    _commandDatabase.Add(row[1] + ".input", row[2].ToString());
                }
            }
        }
        foreach (var pair in _commandRegister)
        {
            var commandId = pair.Key;
            var commandStringedId = pair.Value;
            var commandInput = _commandDatabase[commandStringedId + ".input"].Split(',');
            foreach (var command in commandInput) _petCommands.Add(command, new PetCommand(commandId, command));
        }
    }

    public int TryInvoke(string input)
    {
        PetCommand command = null;
        if (_petCommands.TryGetValue(input.ToLower(), out command))
            return command.Id;
        return 0;
    }
}