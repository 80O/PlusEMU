using NLog;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.Core;

public static class ConsoleCommands
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Core.ConsoleCommands");

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
                    Log.Warn("The server is saving users furniture, rooms, etc. WAIT FOR THE SERVER TO CLOSE, DO NOT EXIT THE PROCESS IN TASK MANAGER!!");
                    PlusEnvironment.PerformShutDown();
                    break;
                }
                case "alert":
                {
                    var notice = inputData.Substring(6);
                    PlusEnvironment.GetGame().GetClientManager()
                        .SendPacket(new BroadcastMessageAlertComposer(PlusEnvironment.GetLanguageManager().TryGetValue("server.console.alert") + "\n\n" + notice));
                    Log.Info("Alert successfully sent.");
                    break;
                }
                default:
                {
                    Log.Error(parameters[0].ToLower() + " is an unknown or unsupported command. Type help for more information");
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Error in command [" + inputData + "]: " + e);
        }
    }
}