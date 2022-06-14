namespace Plus.Communication.Rcon.Commands.Hotel;

internal class ReloadItemsCommand : IRconCommand
{
    public string Description => "This command is used to reload the game items.";

    public string Key => "reload_items";
    public string Parameters => "";

    public Task<bool> TryExecute(string[] parameters)
    {
        PlusEnvironment.GetGame().GetItemManager().Init();
        return Task.FromResult(true);
    }
}