using Plus.Communication.Rcon.Commands;

namespace Plus.Communication.Rcon
{
    public interface IRconSocket
    {
        void Init(string host, int port, IEnumerable<string> allowedConnections);
        ICommandManager GetCommands();
    }
}