namespace Plus.Communication.Rcon.Commands.Hotel;

internal class ReloadNavigatorCommand : IRconCommand
{
    public string Description => "This command is used to reload the navigator.";

    public string Key => "reload_navigator";
    public string Parameters => "";

    public Task<bool> TryExecute(string[] parameters)
    {
        PlusEnvironment.GetGame().GetNavigator().Init();
        return Task.FromResult(true);
    }
}