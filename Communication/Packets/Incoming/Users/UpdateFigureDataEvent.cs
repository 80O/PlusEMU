using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Users.Clothing;
using Plus.Database;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Incoming.Users;

internal class UpdateFigureDataEvent : IPacketEvent
{
    private readonly IFigureDataManager _figureManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IQuestManager _questManager;
    private readonly IDatabase _database;

    public UpdateFigureDataEvent(IFigureDataManager figureDataManager, IAchievementManager achievementManager, IQuestManager questManager, IDatabase database)
    {
        _figureManager = figureDataManager;
        _achievementManager = achievementManager;
        _questManager = questManager;
        _database = database;
    }

    public Task Parse(GameClient session, IIncomingPacket packet)
    {
        var gender = packet.ReadString().ToUpper();
        //TODO get habboclub status
        var look = _figureManager.ValidateLookAsync(packet.ReadString(), gender, session.GetHabbo().Clothing.GetClothingParts, true).Result;
        if (look == session.GetHabbo().Look)
            return Task.CompletedTask;
        if ((DateTime.Now - session.GetHabbo().LastClothingUpdateTime).TotalSeconds <= 2.0)
        {
            session.GetHabbo().ClothingUpdateWarnings += 1;
            if (session.GetHabbo().ClothingUpdateWarnings >= 25)
                session.GetHabbo().SessionClothingBlocked = true;
            return Task.CompletedTask;
        }
        if (session.GetHabbo().SessionClothingBlocked)
            return Task.CompletedTask;
        session.GetHabbo().LastClothingUpdateTime = DateTime.Now;
        string[] allowedGenders = { "M", "F" };
        if (!allowedGenders.Contains(gender))
        {
            session.Send(new BroadcastMessageAlertComposer("Sorry, you chose an invalid gender."));
            return Task.CompletedTask;
        }
        _questManager.ProgressUserQuest(session, QuestType.ProfileChangeLook);
        session.GetHabbo().Look = look;
        session.GetHabbo().Gender = gender.ToLower();
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery($"UPDATE `users` SET `look` = @look, `gender` = @gender WHERE `id` = '{session.GetHabbo().Id}' LIMIT 1");
            dbClient.AddParameter("look", look);
            dbClient.AddParameter("gender", gender);
            dbClient.RunQuery();
        }
        _achievementManager.ProgressAchievement(session, "ACH_AvatarLooks", 1);
        session.Send(new AvatarAspectUpdateComposer(look, gender));
        if (session.GetHabbo().Look.Contains("ha-1006"))
            _questManager.ProgressUserQuest(session, QuestType.WearHat);
        if (session.GetHabbo().InRoom)
        {
            var roomUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser != null)
            {
                session.Send(new UserChangeComposer(roomUser, true));
                session.GetHabbo().CurrentRoom.SendPacket(new UserChangeComposer(roomUser, false));
            }
        }
        return Task.CompletedTask;
    }
}