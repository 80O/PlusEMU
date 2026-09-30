using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Interactor;

internal sealed class InteractorSkateboard : IFurniInteractor
{
    private const int SkateboardEffect = 71;

    public void OnWalkOn(RoomUser user)
    {
        var session = user.GetClient();
        var habbo = session?.GetHabbo();
        if (habbo?.Effects == null)
            return;

        if (habbo.Effects.CurrentEffect != SkateboardEffect)
            habbo.Effects.ApplyEffect(SkateboardEffect);

        if (user.LastItem?.Definition.ItemName != "sb_rail")
            return;

        if (user.LastItem.Rotation == 2)
        {
            user.RotBody = 3;
            user.RotHead = 3;
            user.Z += 1;
            PlusEnvironment.Game.AchievementManager.ProgressAchievement(session, "ACH_SkateBoardJump", 1);
        }
        else if (user.LastItem.Rotation == 0)
        {
            user.RotBody = 2;
            user.RotHead = 2;
            PlusEnvironment.Game.AchievementManager.ProgressAchievement(session, "ACH_SkateBoardSlide", 1);
        }
        else
        {
            return;
        }

        user.UpdateNeeded = true;
    }
}
