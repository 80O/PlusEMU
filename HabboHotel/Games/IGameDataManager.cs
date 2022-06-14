namespace Plus.HabboHotel.Games;

public interface IGameDataManager
{
    ICollection<GameData> GameData { get; }
    void Init();
    bool TryGetGame(int gameId, out GameData data);
    int GetCount();
}