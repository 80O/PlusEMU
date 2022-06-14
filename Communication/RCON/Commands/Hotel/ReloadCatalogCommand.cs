using Plus.Communication.Packets.Outgoing.Catalog;

namespace Plus.Communication.Rcon.Commands.Hotel;

internal class ReloadCatalogCommand : IRconCommand
{
    public string Description => "This command is used to reload the catalog.";

    public string Key => "reload_catalog";
    public string Parameters => "";

    public Task<bool> TryExecute(string[] parameters)
    {
        PlusEnvironment.GetGame().GetCatalog().Init(PlusEnvironment.GetGame().GetItemManager());
        PlusEnvironment.GetGame().GetClientManager().SendPacket(new CatalogUpdatedComposer());
        return Task.FromResult(true);
    }
}