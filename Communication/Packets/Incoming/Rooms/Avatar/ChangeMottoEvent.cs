using System;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms.Chat.Filter;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar;

internal class ChangeMottoEvent : IPacketEvent
{
    private readonly IWordFilterManager _wordFilterManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IQuestManager _questManager;
    private readonly IDatabase _database;

    public ChangeMottoEvent(IWordFilterManager wordFilterManager, IAchievementManager achievementManager, IQuestManager questManager, IDatabase database)
    {
        _wordFilterManager = wordFilterManager;
        _achievementManager = achievementManager;
        _questManager = questManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (session.GetHabbo().TimeMuted > 0)
        {
            session.SendNotification("Oops, you're currently muted - you cannot change your motto.");
            return Task.CompletedTask;
        }
        if ((DateTime.Now - session.GetHabbo().LastMottoUpdateTime).TotalSeconds <= 2.0)
        {
            session.GetHabbo().MottoUpdateWarnings += 1;
            if (session.GetHabbo().MottoUpdateWarnings >= 25)
                session.GetHabbo().SessionMottoBlocked = true;
            return Task.CompletedTask;
        }
        if (session.GetHabbo().SessionMottoBlocked)
            return Task.CompletedTask;
        session.GetHabbo().LastMottoUpdateTime = DateTime.Now;
        var newMotto = StringCharFilter.Escape(packet.PopString().Trim());
        if (newMotto.Length > 38)
            newMotto = newMotto.Substring(0, 38);
        if (newMotto == session.GetHabbo().Motto)
            return Task.CompletedTask;
        if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
            newMotto = _wordFilterManager.CheckMessage(newMotto);
        session.GetHabbo().Motto = newMotto;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE `users` SET `motto` = @motto WHERE `id` = @userId LIMIT 1");
            dbClient.AddParameter("userId", session.GetHabbo().Id);
            dbClient.AddParameter("motto", newMotto);
            dbClient.RunQuery();
        }
        _questManager.ProgressUserQuest(session, QuestType.ProfileChangeMotto);
        _achievementManager.ProgressAchievement(session, "ACH_Motto", 1);
        if (session.GetHabbo().InRoom)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return Task.CompletedTask;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null || user.GetClient() == null)
                return Task.CompletedTask;
            room.SendPacket(new UserChangeComposer(user, false));
        }
        return Task.CompletedTask;
    }
}