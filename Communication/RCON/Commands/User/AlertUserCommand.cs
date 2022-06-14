using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.Communication.Rcon.Commands.User;

internal class AlertUserCommand : IRconCommand
{
    public string Key => "alert_user";
    public string Description => "This command is used to alert a user.";

    public string Parameters => "%userId% %message%";

    public Task<bool> TryExecute(string[] parameters)
    {
        if (!int.TryParse(parameters[0], out var userId))
            return Task.FromResult(false);
        var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null)
            return Task.FromResult(false);

        // Validate the message
        if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            return Task.FromResult(false);
        var message = Convert.ToString(parameters[1]);
        client.SendPacket(new BroadcastMessageAlertComposer(message));
        return Task.FromResult(true);
    }
}