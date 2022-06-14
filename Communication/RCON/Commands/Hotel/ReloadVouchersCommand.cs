namespace Plus.Communication.Rcon.Commands.Hotel;

internal class ReloadVouchersCommand : IRconCommand
{
    public string Description => "This command is used to reload the voucher manager.";

    public string Key => "reload_vouchers";
    public string Parameters => "";

    public Task<bool> TryExecute(string[] parameters)
    {
        PlusEnvironment.GetGame().GetCatalog().GetVoucherManager().Init();
        return Task.FromResult(true);
    }
}