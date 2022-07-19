﻿using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Flash
{
    public interface IGameServer
    {
        bool Start();
        bool Stop();

        Task PacketReceived(GameClient client, uint messageId, IIncomingPacket packet);
    }
}