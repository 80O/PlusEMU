using NLog;
using Plus.Communication;
using Plus.Communication.ConnectionManager;
using Plus.Communication.Encryption.Crypto.Prng;
using Plus.Communication.Interfaces;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Core;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.GameClients;

public class GameClient
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    private ConnectionInformation _connection;
    private bool _disconnected;
    private Habbo? _habbo;
    private GamePacketParser _packetParser;
    public string MachineId = string.Empty;
    public Arc4? Rc4Client;

    public GameClient(int clientId, ConnectionInformation connection)
    {
        ConnectionId = clientId;
        _connection = connection;
        _packetParser = new GamePacketParser(this);
        PingCount = 0;
    }

    public int PingCount { get; set; }

    public int ConnectionId { get; }

    private void SwitchParserRequest()
    {
        _packetParser.SetConnection(_connection);
        _packetParser.OnNewPacket += ParserOnNewPacket;
        var data = (_connection.Parser as InitialPacketParser).CurrentData;
        _connection.Parser.Dispose();
        _connection.Parser = _packetParser;
        _connection.Parser.HandlePacketData(data);
    }

    private void ParserOnNewPacket(ClientPacket message)
    {
        try
        {
            PlusEnvironment.GetGame().GetPacketManager().TryExecutePacket(this, message);
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
    }

    private void PolicyRequest()
    {
        _connection.SendData(PlusEnvironment.GetDefaultEncoding().GetBytes("<?xml version=\"1.0\"?>\r\n" +
                                                                           "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                                                                           "<cross-domain-policy>\r\n" +
                                                                           "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
                                                                           "</cross-domain-policy>\x0"));
    }


    public void StartConnection()
    {
        PingCount = 0;
        (_connection.Parser as InitialPacketParser).PolicyRequest += PolicyRequest;
        (_connection.Parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
        _connection.StartPacketProcessing();
    }

    public void SendWhisper(string message, int colour = 0)
    {
        if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
            return;
        var user = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Username);
        if (user == null)
            return;
        SendPacket(new WhisperComposer(user.VirtualId, message, 0, colour == 0 ? user.LastBubble : colour));
    }

    public void SendNotification(string message) => SendPacket(new BroadcastMessageAlertComposer(message));

    public void SendPacket(IServerPacket message)
    {
        Log.Debug("Sending Packet [{packetId}]({name}) to user [{userId}]", message.Id, message.GetType().Name, GetHabbo()?.Id);
        GetConnection().SendData(message.GetBytes());
    }

    public ConnectionInformation GetConnection() => _connection;

    public Habbo GetHabbo() => _habbo!;

    public void SetHabbo(Habbo habbo)
    {
        ArgumentNullException.ThrowIfNull(habbo);
        if (_habbo != null) throw new InvalidOperationException("Habbo already set");
        _habbo = habbo;
    }

    public void Disconnect()
    {
        try
        {
            if (GetHabbo() != null)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery(GetHabbo().GetQueryString);
                }
                GetHabbo().OnDisconnect();
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
        if (!_disconnected)
        {
            if (_connection != null)
                _connection.Dispose();
            _disconnected = true;
        }
    }

    public void Dispose()
    {
        if (GetHabbo() != null)
            GetHabbo().OnDisconnect();
        MachineId = string.Empty;
        _disconnected = true;
        _habbo = null;
        _connection = null;
        Rc4Client = null;
        _packetParser = null;
    }
}
