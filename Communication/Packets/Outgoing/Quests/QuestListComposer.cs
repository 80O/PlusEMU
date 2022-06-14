using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Outgoing.Quests;

public class QuestListComposer : ServerPacket
{
    public QuestListComposer(GameClient session, bool send, Dictionary<string, Quest> userQuests)
        : base(ServerPacketHeader.QuestListMessageComposer)
    {
        WriteInteger(userQuests.Count);

        // Active ones first
        foreach (var userQuest in userQuests)
        {
            if (userQuest.Value == null)
                continue;
            SerializeQuest(this, session, userQuest.Value, userQuest.Key);
        }

        // Dead ones last
        foreach (var userQuest in userQuests)
        {
            if (userQuest.Value != null)
                continue;
            SerializeQuest(this, session, userQuest.Value, userQuest.Key);
        }
        WriteBoolean(send);
    }

    private void SerializeQuest(ServerPacket message, GameClient session, Quest quest, string category)
    {
        if (message == null || session == null)
            return;
        var amountInCat = PlusEnvironment.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(category);
        var number = quest == null ? amountInCat : quest.Number - 1;
        var userProgress = quest == null ? 0 : session.GetHabbo().GetQuestProgress(quest.Id);
        if (quest != null && quest.IsCompleted(userProgress))
            number++;
        message.WriteString(category);
        message.WriteInteger(quest == null ? 0 : quest.Category.Contains("xmas2012") ? 0 : number); // Quest progress in this cat
        message.WriteInteger(quest == null ? 0 : quest.Category.Contains("xmas2012") ? 0 : amountInCat); // Total quests in this cat
        message.WriteInteger(quest?.RewardType ?? 3); // Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
        message.WriteInteger(quest?.Id ?? 0); // Quest id
        message.WriteBoolean(quest == null ? false : session.GetHabbo().GetStats().QuestId == quest.Id); // Quest started
        message.WriteString(quest == null ? string.Empty : quest.ActionName);
        message.WriteString(quest == null ? string.Empty : quest.DataBit);
        message.WriteInteger(quest?.Reward ?? 0);
        message.WriteString(quest == null ? string.Empty : quest.Name);
        message.WriteInteger(userProgress); // Current progress
        message.WriteInteger(quest?.GoalData ?? 0); // Target progress
        message.WriteInteger(quest?.TimeUnlock ?? 0); // "Next quest available countdown" in seconds
        message.WriteString("");
        message.WriteString("");
        message.WriteBoolean(true);
    }
}