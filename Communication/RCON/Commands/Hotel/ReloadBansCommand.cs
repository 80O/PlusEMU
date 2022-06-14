namespace Plus.Communication.Rcon.Commands.Hotel;

internal class ReloadBansCommand : IRconCommand
{
    public string Description => "This command is used to re-cache the bans.";

    public string Key => "reload_bans";
    public string Parameters => "";

    public Task<bool> TryExecute(string[] parameters)
    {
        PlusEnvironment.GetGame().GetModerationManager().ReCacheBans();
        return Task.FromResult(true);
    }
}