﻿using Plus.Communication.Packets;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.HabboHotel.GameClients;

public interface IGameClientManager
{
    int Count { get; }
    ICollection<GameClient> GetClients { get; }
    void OnCycle();
    GameClient? GetClientByUserId(int userId);
    GameClient? GetClientByUsername(string username);
    bool TryGetClient(int clientId, out GameClient client);
    bool UpdateClientUsername(GameClient client, string oldUsername, string newUsername);
    Task<string> GetNameById(int id);
    IEnumerable<GameClient> GetClientsById(Dictionary<int, MessengerBuddy>.KeyCollection users);
    void StaffAlert(IServerPacket message, int exclude = 0);
    void ModAlert(string message);
    void DoAdvertisingReport(GameClient reporter, GameClient target);
    void SendPacket(IServerPacket packet, string fuse = "");
    void DisposeConnection(int clientId);
    void LogClonesOut(int userId);
    void RegisterClient(GameClient client, int userId, string username);
    void UnregisterClient(int userid, string username);
    void CloseAll();
}