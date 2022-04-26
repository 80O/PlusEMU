using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Incoming.Quests;

internal class GetCurrentQuestEvent : IPacketEvent
{
    private readonly IQuestManager _questManager;
    private readonly IDatabase _database;

    public GetCurrentQuestEvent(IQuestManager questManager, IDatabase database)
    {
        _questManager = questManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var userQuest = _questManager.GetQuest(session.GetHabbo().QuestLastCompleted);
        var nextQuest = _questManager.GetNextQuestInSeries(userQuest.Category, userQuest.Number + 1);
        if (nextQuest == null)
            return Task.CompletedTask;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery("REPLACE INTO `user_quests`(`user_id`,`quest_id`) VALUES (" + session.GetHabbo().Id + ", " + nextQuest.Id + ")");
            dbClient.RunQuery("UPDATE `user_stats` SET `quest_id` = '" + nextQuest.Id + "' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
        }
        session.GetHabbo().GetStats().QuestId = nextQuest.Id;
        _questManager.GetList(session, null);
        session.SendPacket(new QuestStartedComposer(session, nextQuest));
        return Task.CompletedTask;
    }
}