using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using NetCoreServer;
using Plus.Communication.Flash;
using Plus.Communication.Packets;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Abstractions
{
    public abstract class WebsocketGameServer<TGameServerOptions> : WsServer, IGameServer
        where TGameServerOptions : class, IGameServerOptions
    {
        private readonly IGameClientFactory<WsSessionProxy, WsServer> _clientFactory;
        private readonly IPacketManager _packetManager;
        private readonly ConcurrentDictionary<Guid, WsSessionProxy> _connectedClients = new();

        protected WebsocketGameServer(IOptions<TGameServerOptions> options,
            IGameClientFactory<WsSessionProxy, WsServer> clientFactory,
            IPacketManager packetManager) : base(options.Value.Hostname,
            options.Value.Port)
        {
            _clientFactory = clientFactory;
            _packetManager = packetManager;
        }

        protected override WsSession CreateSession() => _clientFactory.Create(this);

        protected override void OnConnected(TcpSession session)
        {
            if (session is not WsSessionProxy gameClient)
            {
                session.Disconnect();
                //_logger.LogWarning("Expected {TGameClient} to be connected. Got {type}", typeof(TGameClient), session.GetType());
                return;
            }

            if (!_connectedClients.TryAdd(gameClient.Id, gameClient))
            {
                //_logger.LogWarning("Failed to cache client. {id} {ip}", gameClient.Id, gameClient.Socket.RemoteEndPoint?.ToString());
                gameClient.Disconnect();
            }
        }

        protected override void OnDisconnected(TcpSession session)
        {
            _connectedClients.TryRemove(session.Id, out _);
        }


        // TODO @80O: Allow packet content to be modified before executing.
        // TODO @80O: Add hooks before & after packet execution.
        public Task PacketReceived(GameClient client, uint messageId, IIncomingPacket packet) => _packetManager.TryExecutePacket(client, messageId, packet);
    }
}