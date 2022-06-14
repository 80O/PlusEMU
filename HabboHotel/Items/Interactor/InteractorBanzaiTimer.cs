using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Items.Interactor;

public class InteractorBanzaiTimer : IFurniInteractor
{
    public void OnPlace(GameClient session, Item item) { }

    public void OnRemove(GameClient session, Item item) { }

    public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
    {
        if (!hasRights) return;
        var oldValue = 0;
        if (!int.TryParse(item.ExtraData, out oldValue))
        {
            item.ExtraData = "0";
            oldValue = 0;
        }
        if (request == 0 && oldValue == 0)
            oldValue = 30;
        else if (request == 2)
        {
            if (item.GetRoom().GetBanzai().IsBanzaiActive && item.PendingReset && oldValue > 0)
            {
                oldValue = 0;
                item.PendingReset = false;
            }
            else
            {
                if (oldValue < 30)
                    oldValue = 30;
                else if (oldValue == 30)
                    oldValue = 60;
                else if (oldValue == 60)
                    oldValue = 120;
                else if (oldValue == 120)
                    oldValue = 180;
                else if (oldValue == 180)
                    oldValue = 300;
                else if (oldValue == 300)
                    oldValue = 600;
                else
                    oldValue = 0;
                item.UpdateNeeded = false;
            }
        }
        else if (request == 1 || request == 0)
        {
            if (request == 1 && oldValue == 0)
            {
                item.ExtraData = "30";
                oldValue = 30;
            }
            if (!item.GetRoom().GetBanzai().IsBanzaiActive)
            {
                item.UpdateNeeded = !item.UpdateNeeded;
                if (item.UpdateNeeded) item.GetRoom().GetBanzai().BanzaiStart();
                item.PendingReset = true;
            }
            else
            {
                item.UpdateNeeded = !item.UpdateNeeded;
                if (item.UpdateNeeded) item.GetRoom().GetBanzai().BanzaiEnd(true);
                item.PendingReset = true;
            }
        }
        item.ExtraData = Convert.ToString(oldValue);
        item.UpdateState();
    }

    public void OnWiredTrigger(Item item)
    {
        if (item.GetRoom().GetBanzai().IsBanzaiActive)
            item.GetRoom().GetBanzai().BanzaiEnd(true);
        item.PendingReset = true;
        item.UpdateNeeded = true;
        item.ExtraData = "30";
        item.UpdateState();
        if (!item.GetRoom().GetBanzai().IsBanzaiActive)
            item.GetRoom().GetBanzai().BanzaiStart();
    }
}