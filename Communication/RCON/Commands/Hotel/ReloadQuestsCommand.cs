namespace Plus.Communication.Rcon.Commands.Hotel;

internal class ReloadQuestsCommand : IRconCommand
{
    public string Description => "This command is used to reload the quests manager.";

    public string Key => "reload_quests";
    public string Parameters => "";

    public Task<bool> TryExecute(string[] parameters)
    {
        PlusEnvironment.GetGame().GetQuestManager().Init();
        return Task.FromResult(true);
    }
}