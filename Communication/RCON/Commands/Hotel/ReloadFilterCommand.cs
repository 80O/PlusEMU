namespace Plus.Communication.Rcon.Commands.Hotel;

internal class ReloadFilterCommand : IRconCommand
{
    public string Description => "This command is used to reload the chatting filter manager.";

    public string Key => "reload_filter";
    public string Parameters => "";

    public Task<bool> TryExecute(string[] parameters)
    {
        PlusEnvironment.GetGame().GetChatManager().GetFilter().Init();
        return Task.FromResult(true);
    }
}