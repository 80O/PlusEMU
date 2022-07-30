using Microsoft.Extensions.Logging;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.Core;

internal class ConsoleCommands
{
    private readonly ILogger<ConsoleCommands> _logger;

    public ConsoleCommands(ILogger<ConsoleCommands> logger)
    {
        _logger = logger;
    }

    public static void InvokeCommand(string inputData)
    {
        if (string.IsNullOrEmpty(inputData))
            return;
        try
        {
            var parameters = inputData.Split(' ');
            switch (parameters[0].ToLower())
            {
                case "stop":
                case "shutdown":
                {
                    _logger.Warn("The server is saving users furniture, rooms, etc. WAIT FOR THE SERVER TO CLOSE, DO NOT EXIT THE PROCESS IN TASK MANAGER!!");
                    PlusEnvironment.PerformShutDown();
                    break;
                }
                case "alert":
                {
                    var notice = inputData.Substring(6);
                    PlusEnvironment.GetGame().GetClientManager().SendPacket(new BroadcastMessageAlertComposer(PlusEnvironment.GetLanguageManager().TryGetValue("server.console.alert") + "\n\n" + notice));
                    _logger.Info("Alert successfully sent.");
                    break;
                }
                default:
                {
                    _logger.Error(parameters[0].ToLower() + " is an unknown or unsupported command. Type help for more information");
                    break;
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error("Error in command [" + inputData + "]: " + e);
        }
    }
}