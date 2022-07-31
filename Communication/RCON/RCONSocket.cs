using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Plus.Communication.Rcon.Commands;

namespace Plus.Communication.Rcon;

public class RconSocket : IRconSocket
{
    private readonly ILogger<RconSocket> _logger;
    private List<string> _allowedConnections;
    private readonly ICommandManager _commands;
    private Socket _musSocket;

    public RconSocket(ICommandManager commandManager, ILogger<RconSocket> logger)
    {
        _commands = commandManager;
        _logger = logger;
    }

    public void Init(string host, int port, IEnumerable<string> allowedConnections)
    {
        _allowedConnections = new List<string>();
        foreach (var ipAddress in allowedConnections) _allowedConnections.Add(ipAddress);
        try
        {
            _musSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _musSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port)); // SHould be host?
            _musSocket.Listen(0);
            _musSocket.BeginAccept(OnCallBack, _musSocket);
        }
        catch (Exception e)
        {
            throw new ArgumentException("Could not set up Rcon socket:\n" + e);
        }
    }

    private void OnCallBack(IAsyncResult iAr)
    {
        try
        {
            var socket = ((Socket)iAr.AsyncState).EndAccept(iAr);
            var ip = socket.RemoteEndPoint.ToString().Split(':')[0];
            if (_allowedConnections.Contains(ip))
                new RconConnection(socket, _logger);
            else
                socket.Close();
        }
        catch (Exception)
        {
            // ignored
        }
        _musSocket.BeginAccept(OnCallBack, _musSocket);
    }

    public ICommandManager GetCommands() => _commands;
}