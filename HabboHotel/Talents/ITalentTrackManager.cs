namespace Plus.HabboHotel.Talents;

public interface ITalentTrackManager
{
    void Init();
    ICollection<TalentTrackLevel> GetLevels();
}