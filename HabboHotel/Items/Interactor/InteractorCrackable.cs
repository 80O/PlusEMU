using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.DataFormat;

namespace Plus.HabboHotel.Items.Interactor;

internal sealed class InteractorCrackable : IFurniInteractor
{
    private const int RequiredEffect = 158;

    public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
    {
        if (session?.GetHabbo()?.Effects == null || session.GetHabbo().Effects.CurrentEffect != RequiredEffect)
            return;
        if (item.ExtraData is not CrackableDataFormat data || data.Target == 0 || data.Hits >= data.Target)
            return;

        PlusEnvironment.Game.AchievementManager.ProgressAchievement(session, "ACH_PinataWhacker", 1);
        if (data.TryCrack())
            PlusEnvironment.Game.AchievementManager.ProgressAchievement(session, "ACH_PinataBreaker", 1);
        item.UpdateState();
    }
}
