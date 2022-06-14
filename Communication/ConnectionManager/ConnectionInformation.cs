using System.Net.Sockets;

namespace Plus.Communication.ConnectionManager;

public class ConnectionInformation : IDisposable
{
    /// <summary>
    ///     Is used when a connection state changes
    /// </summary>
    /// <param name="state">The new state of the connection</param>
    public delegate void ConnectionChange(ConnectionInformation information, ConnectionState state);

    private const bool DisableSend = false;
    private const bool DisableReceive = false;

    /// <summary>
    ///     Buffer of the connection
    /// </summary>
    private readonly byte[] _buffer;

    /// <summary>
    ///     The id of this connection
    /// </summary>
    private readonly int _connectionId;

    /// <summary>
    ///     The socket this connection is based upon
    /// </summary>
    private readonly Socket _dataSocket;

    /// <summary>
    ///     The ip of this connection
    /// </summary>
    private readonly string _ip;

    private readonly AsyncCallback _sendCallback;

    /// <summary>
    ///     Boolean indicating of this instance is connected to the user or not
    /// </summary>
    private bool _isConnected;

    /// <summary>
    ///     Creates a new Connection witht he given information
    /// </summary>
    /// <param name="dataStream">The Socket of the connection</param>
    /// <param name="connectionId">The id of the connection</param>
    /// <param name="parser">The data parser for the connection</param>
    /// <param name="ip">The IP Address for the connection</param>
    public ConnectionInformation(int connectionId, Socket dataStream, IDataParser parser, string ip)
    {
        Parser = parser;
        _buffer = new byte[GameSocketManagerStatics.BufferSize];
        _dataSocket = dataStream;
        _dataSocket.SendBufferSize = GameSocketManagerStatics.BufferSize;
        _ip = ip;
        _sendCallback = SentData;
        _connectionId = connectionId;
        if (ConnectionChanged != null)
            ConnectionChanged.Invoke(this, ConnectionState.Open);
    }

    /// <summary>
    ///     This item contains the data parser for the connection
    /// </summary>
    public IDataParser Parser { get; set; }

    /// <summary>
    ///     Disposes the current item
    /// </summary>
    public void Dispose()
    {
        if (_isConnected) Disconnect();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Is triggered when the user connects/disconnects
    /// </summary>
    public event ConnectionChange ConnectionChanged;

    /// <summary>
    ///     Starts this item packet processor
    ///     MUST be called before sending data
    /// </summary>
    public void StartPacketProcessing()
    {
        if (!_isConnected)
        {
            _isConnected = true;
            //Out.writeLine("Starting packet processsing of client [" + this.connectionID + "]", Out.logFlags.lowLogLevel);
            try
            {
                _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, IncomingDataPacket, _dataSocket);
            }
            catch
            {
                Disconnect();
            }
        }
    }

    /// <summary>
    ///     Returns the ip of the current connection
    /// </summary>
    /// <returns>The ip of this connection</returns>
    public string GetIp() => _ip;

    /// <summary>
    ///     Returns the connection id
    /// </summary>
    /// <returns>The id of the connection</returns>
    public int GetConnectionId() => _connectionId;

    /// <summary>
    ///     Disconnects the current connection
    /// </summary>
    public void Disconnect()
    {
        try
        {
            if (_isConnected)
            {
                _isConnected = false;

                //Out.writeLine("Connection [" + this.connectionID + "] has been disconnected", Out.logFlags.BelowStandardlogLevel);
                try
                {
                    if (_dataSocket != null && _dataSocket.Connected)
                    {
                        _dataSocket.Shutdown(SocketShutdown.Both);
                        _dataSocket.Close();
                    }
                }
                catch
                {
                    //ignored
                }
                _dataSocket.Dispose();
                Parser.Dispose();
                try
                {
                    if (ConnectionChanged != null)
                        ConnectionChanged.Invoke(this, ConnectionState.Closed);
                }
                catch
                {
                    //ignored
                }
                ConnectionChanged = null;
            }
        }
        catch { }
    }

    /// <summary>
    ///     Receives a packet of data and processes it
    /// </summary>
    /// <param name="iAr">The interface of an async result</param>
    private void IncomingDataPacket(IAsyncResult iAr)
    {
        //Out.writeLine("Packet received from client [" + this.connectionID + "]", Out.logFlags.lowLogLevel);
        int bytesReceived;
        try
        {
            //The amount of bytes received in the packet
            bytesReceived = _dataSocket.EndReceive(iAr);
        }
        catch (Exception)
        {
            Disconnect();
            return;
        }
        if (bytesReceived == 0)
        {
            //   Disconnect();
            return;
        }
        try
        {
            if (!DisableReceive)
            {
                var packet = new byte[bytesReceived];
                Array.Copy(_buffer, packet, bytesReceived);
                HandlePacketData(packet);
            }
        }
        catch //(Exception e)
        {
            Disconnect();
        }
        finally
        {
            try
            {
                //and we keep looking for the next packet
                _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, IncomingDataPacket, _dataSocket);
            }
            catch //(Exception e)
            {
                Disconnect();
            }
        }
    }

    /// <summary>
    ///     Handles packet data
    /// </summary>
    /// <param name="packet">The data received by the </param>
    private void HandlePacketData(byte[] packet)
    {
        if (Parser != null) Parser.HandlePacketData(packet);
    }

    public void SendData(byte[] packet)
    {
        try
        {
            if (!_isConnected || DisableSend)
                return;

            //Console.WriteLine(string.Format("Data from server => [{0}]", packetData));
            _dataSocket.BeginSend(packet, 0, packet.Length, 0, _sendCallback, null);
        }
        catch
        {
            Disconnect();
        }
    }

    /// <summary>
    ///     Same as sendData
    /// </summary>
    /// <param name="iAr">The a-synchronious interface</param>
    private void SentData(IAsyncResult iAr)
    {
        try
        {
            _dataSocket.EndSend(iAr);
        }
        catch
        {
            Disconnect();
        }
    }
}