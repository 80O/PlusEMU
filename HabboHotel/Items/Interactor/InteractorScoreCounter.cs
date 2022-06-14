using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Games.Teams;

namespace Plus.HabboHotel.Items.Interactor;

public class InteractorScoreCounter : IFurniInteractor
{
    public void OnPlace(GameClient session, Item item)
    {
        if (item.Team == Team.None)
            return;
        item.ExtraData = item.GetRoom().GetGameManager().Points[Convert.ToInt32(item.Team)].ToString();
        item.UpdateState(false, true);
    }

    public void OnRemove(GameClient session, Item item) { }

    public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
    {
        if (!hasRights) return;
        var oldValue = 0;
        if (!int.TryParse(item.ExtraData, out oldValue)) { }
        if (request == 1)
            oldValue++;
        else if (request == 2)
            oldValue--;
        else if (request == 3) oldValue = 0;
        item.ExtraData = oldValue.ToString();
        item.UpdateState(false, true);
    }

    public void OnWiredTrigger(Item item)
    {
        var oldValue = 0;
        if (!int.TryParse(item.ExtraData, out oldValue)) { }
        oldValue++;
        item.ExtraData = oldValue.ToString();
        item.UpdateState(false, true);
    }
}