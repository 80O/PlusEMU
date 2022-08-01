using NetCoreServer;

namespace Plus.HabboHotel.GameClients
{
    public class WsSessionProxy : WsSession
    {
        private readonly GameClient _client;
        public WsSessionProxy(WsServer server, GameClient client) : base(server)
        {
            _client = client;
            _client.SendCallback = args =>
            {
                if (!Socket.Connected) return false;
                var buffer = args.MemoryBuffer.ToArray();
                return SendBinaryAsync(buffer, 0, buffer.Length);
            };
            _client.DisconnectRequested = () => Disconnect();
        }

        protected override void OnConnected()
        {
            Socket.DontFragment = true;
            base.OnConnected();
        }

        protected override void OnDisconnected() => _client.OnDisconnected();

        public override void OnWsReceived(byte[] buffer, long offset, long size) => _client.OnReceived(buffer, offset, size);
    }
}