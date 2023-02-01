﻿using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.HabboHotel.Items.Interactor;

public class InteractorGenericSwitch : IFurniInteractor
{
    public void OnPlace(GameClient session, Item item) { }

    public void OnRemove(GameClient session, Item item) { }

    public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
    {
        var modes = item.Definition.Modes - 1;
        if (session == null || !hasRights || modes <= 0) return;
        PlusEnvironment.Game.QuestManager.ProgressUserQuest(session, QuestType.FurniSwitch);
        var currentMode = 0;
        var newMode = 0;
        if (!int.TryParse(item.LegacyDataString, out currentMode)) { }
        if (currentMode <= 0)
            newMode = 1;
        else if (currentMode >= modes)
            newMode = 0;
        else
            newMode = currentMode + 1;
        item.LegacyDataString = newMode.ToString();
        item.UpdateState();
    }

    public void OnWiredTrigger(Item item)
    {
        var modes = item.Definition.Modes - 1;
        if (modes == 0) return;
        var currentMode = 0;
        var newMode = 0;
        if (string.IsNullOrEmpty(item.LegacyDataString))
            item.LegacyDataString = "0";
        if (!int.TryParse(item.LegacyDataString, out currentMode)) return;
        if (currentMode <= 0)
            newMode = 1;
        else if (currentMode >= modes)
            newMode = 0;
        else
            newMode = currentMode + 1;
        item.LegacyDataString = newMode.ToString();
        item.UpdateState();
    }
}