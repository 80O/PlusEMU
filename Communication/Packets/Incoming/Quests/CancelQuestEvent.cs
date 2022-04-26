using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Incoming.Quests;

internal class CancelQuestEvent : IPacketEvent
{
    private readonly IQuestManager _questManager;
    private readonly IDatabase _database;

    public CancelQuestEvent(IQuestManager questManager, IDatabase database)
    {
        _questManager = questManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var quest = _questManager.GetQuest(session.GetHabbo().GetStats().QuestId);
        if (quest == null)
            return Task.CompletedTask;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.RunQuery("DELETE FROM `user_quests` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `quest_id` = '" + quest.Id + "';" +
                              "UPDATE `user_stats` SET `quest_id` = '0' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
        }
        session.GetHabbo().GetStats().QuestId = 0;
        session.SendPacket(new QuestAbortedComposer());
        _questManager.GetList(session, null);
        return Task.CompletedTask;
    }
}