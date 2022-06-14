using Plus.Core;

namespace Plus.Communication.ConnectionManager;

public class ConnectionHandling : IConnectionHandling
{
    private readonly ISocketManager _socketManager;

    public ConnectionHandling(ISocketManager socketSocketManager)
    {
        _socketManager = socketSocketManager;
    }

    public void Init(int port, int maxConnections, int connectionsPerIp, bool enabeNagles)
    {
        _socketManager.Init(port, maxConnections, connectionsPerIp, new InitialPacketParser(), !enabeNagles);
        _socketManager.OnConnectionEvent += OnConnectionEvent;
        _socketManager.InitializeConnectionRequests();
    }

    private void OnConnectionEvent(ConnectionInformation connection)
    {
        connection.ConnectionChanged += OnConnectionChanged;
        PlusEnvironment.GetGame().GetClientManager().CreateAndStartClient(Convert.ToInt32(connection.GetConnectionId()), connection);
    }

    private void OnConnectionChanged(ConnectionInformation information, ConnectionState state)
    {
        if (state == ConnectionState.Closed) CloseConnection(information);
    }

    private void CloseConnection(ConnectionInformation connection)
    {
        try
        {
            connection.Dispose();
            PlusEnvironment.GetGame().GetClientManager().DisposeConnection(Convert.ToInt32(connection.GetConnectionId()));
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
    }

    public void Destroy()
    {
        _socketManager.Destroy();
    }
}