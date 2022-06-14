using NLog;
using System.Net.Sockets;
using System.Text;

namespace Plus.Communication.Rcon;

public class RconConnection
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Communication.Rcon.RconConnection");
    private byte[] _buffer = new byte[1024];
    private Socket _socket;

    public RconConnection(Socket socket)
    {
        _socket = socket;
        try
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnCallBack, _socket);
        }
        catch
        {
            Dispose();
        }
    }

    public void OnCallBack(IAsyncResult iAr)
    {
        try
        {
            if (!int.TryParse(_socket.EndReceive(iAr).ToString(), out var bytes))
            {
                Dispose();
                return;
            }
            var data = Encoding.Default.GetString(_buffer, 0, bytes);
            if (!PlusEnvironment.GetRconSocket().GetCommands().Parse(data)) Log.Error("Failed to execute a MUS command. Raw data: " + data);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        Dispose();
    }

    public void Dispose()
    {
        if (_socket != null)
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
        }
        _socket = null;
        _buffer = null;
    }
}