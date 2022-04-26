using NLog;
using Plus.Communication.Attributes;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Plus.Communication.Packets;

public sealed class PacketManager : IPacketManager, IDisposable
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Communication.Packets");

    private readonly Dictionary<int, IPacketEvent> _incomingPackets = new();
    private readonly HashSet<Type> _handshakePackets = new();
    private readonly Dictionary<int, string> _packetNames = new();

    /// <summary>
    ///     The maximum time a task can run for before it is considered dead
    ///     (can be used for debugging any locking issues with certain areas of code)
    /// </summary>
    private readonly TimeSpan _maximumRunTimeInSec; // 5 minutes in debug. 30 seconds in release.
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public PacketManager(IEnumerable<IPacketEvent> incomingPackets)
    {
        _maximumRunTimeInSec = Debugger.IsAttached ? TimeSpan.FromMinutes(30) : TimeSpan.FromSeconds(5);
        foreach (var packet in incomingPackets)
        {
            var field = typeof(ClientPacketHeader).GetField(packet.GetType().Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field == null)
            {
                Log.Warn("No incoming header defined for {packet}", packet.GetType().Name);
                continue;
            }
            var header = (int) field.GetValue(null);
            _incomingPackets.Add(header, packet);
            _packetNames.Add(header, packet.GetType().Name);
            if (packet.GetType().GetCustomAttribute<NoAuthenticationRequiredAttribute>() != null)
                _handshakePackets.Add(packet.GetType());
        }
    }

    public void TryExecutePacket(GameClient session, ClientPacket packet)
    {
        if (!_incomingPackets.TryGetValue(packet.Id, out var pak))
        {
            if (Debugger.IsAttached)
                Log.Debug("Unhandled Packet: " + packet);
            return;
        }

        if (Debugger.IsAttached)
        {
            if (_packetNames.ContainsKey(packet.Id))
                Log.Debug("Handled Packet: [" + packet.Id + "] " + _packetNames[packet.Id]);
            else
                Log.Debug("Handled Packet: [" + packet.Id + "] UnnamedPacketEvent");
        }

        if (!_handshakePackets.Contains(pak.GetType()) && session.GetHabbo() == null)
        {
            Log.Debug($"Session {session.ConnectionId} tried execute packet {packet.Id} but didn't handshake yet.");
            return;
        }

        ExecutePacketAsync(session, packet, pak);
    }

    private void ExecutePacketAsync(GameClient session, ClientPacket packet, IPacketEvent pak)
    {
        if (_cancellationTokenSource.IsCancellationRequested)
            return;

        Task.Run(async () =>
        {
            var task = pak.Parse(session, packet);
            await task.WaitAsync(_maximumRunTimeInSec, _cancellationTokenSource.Token);
        }, _cancellationTokenSource.Token).ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception != null)
            {
                foreach (var e in t.Exception.Flatten().InnerExceptions)
                {
                    Log.Error("Error handling packet {packetId} for session {session} @ Habbo  {username}: {message} {stacktrace}", packet.Id, session.ConnectionId, session.GetHabbo()?.Username ?? string.Empty, e.Message, e.StackTrace);
                    session.Disconnect();
                }
            }
        });
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _incomingPackets.Clear();
        _handshakePackets.Clear();
        _packetNames.Clear();
    }
}