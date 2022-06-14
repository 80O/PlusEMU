using Plus.Communication.ConnectionManager;
using Plus.Communication.Packets.Outgoing;
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
    void StaffAlert(ServerPacket message, int exclude = 0);
    void ModAlert(string message);
    void DoAdvertisingReport(GameClient reporter, GameClient target);
    void SendPacket(ServerPacket packet, string fuse = "");
    void CreateAndStartClient(int clientId, ConnectionInformation connection);
    void DisposeConnection(int clientId);
    void LogClonesOut(int userId);
    void RegisterClient(GameClient client, int userId, string username);
    void UnregisterClient(int userid, string username);
    void CloseAll();
}