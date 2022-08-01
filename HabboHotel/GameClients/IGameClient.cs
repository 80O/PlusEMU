using Plus.Communication.Encryption.Crypto.Prng;
using Plus.Communication.Packets;
using Plus.Communication.Revisions;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.GameClients
{
    public interface IGameClient
    {
        event EventHandler<EventArgs>? ConnectionConnected;
        event EventHandler<EventArgs>? ConnectionDisconnected;
        Arc4? Rc4Client { get; set; }
        bool IsAuthenticated { get; set; }
        DateTime TimeConnected { get; set; }
        string MachineId { get; set; }
        int PingCount { get; set; }
        Revision Revision { get; set; }
        Habbo GetHabbo();
        void SetHabbo(Habbo habbo);
        void Send(IServerPacket composer);
        bool Disconnect();
    }
}