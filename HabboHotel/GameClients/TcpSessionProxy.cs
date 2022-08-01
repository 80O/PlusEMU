using NetCoreServer;

namespace Plus.HabboHotel.GameClients
{
    public class TcpSessionProxy : TcpSession
    {
        private readonly GameClient _client;
        public TcpSessionProxy(TcpServer server, GameClient client) : base(server)
        {
            _client = client;
            _client.Id = Id;
            _client.SendCallback = args =>
            {
                if (!Socket.Connected) return false;
                try
                {
                    return Socket.SendAsync(args);
                }
                catch (Exception e) // TODO 80O: Maybe handle some potential errors.
                {
                }
                return false;
            };
            _client.DisconnectRequested = () => Disconnect();
        }

        protected override void OnConnected()
        {
            Socket.DontFragment = true;
            base.OnConnected();
        }

        protected override void OnDisconnected() => _client.OnDisconnected();

        protected override void OnReceived(byte[] buffer, long offset, long size) => _client.OnReceived(buffer, offset, size);
    }
}