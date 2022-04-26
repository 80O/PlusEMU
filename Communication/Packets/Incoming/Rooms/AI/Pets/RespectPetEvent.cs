using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets;

internal class RespectPetEvent : IPacketEvent
{
    private readonly IRoomManager _roomManager;
    private readonly IAchievementManager _achievementManager;
    private readonly IQuestManager _questManager;

    public RespectPetEvent(IRoomManager roomManager, IAchievementManager achievementManager, IQuestManager questManager)
    {
        _roomManager = roomManager;
        _achievementManager = achievementManager;
        _questManager = questManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom || session.GetHabbo().GetStats() == null || session.GetHabbo().GetStats().DailyPetRespectPoints == 0)
            return Task.CompletedTask;
        if (!_roomManager.TryGetRoom(session.GetHabbo().CurrentRoomId, out var room))
            return Task.CompletedTask;
        var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (thisUser == null)
            return Task.CompletedTask;
        var petId = packet.PopInt();
        if (!session.GetHabbo().CurrentRoom.GetRoomUserManager().TryGetPet(petId, out var pet))
        {
            //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
            var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(petId);
            if (targetUser == null)
                return Task.CompletedTask;

            //Check some values first, please!
            if (targetUser.GetClient() == null || targetUser.GetClient().GetHabbo() == null)
                return Task.CompletedTask;
            if (targetUser.GetClient().GetHabbo().Id == session.GetHabbo().Id)
            {
                session.SendWhisper("Oops, you cannot use this on yourself! (You haven't lost a point, simply reload!)");
                return Task.CompletedTask;
            }

            //And boom! Let us send some respect points.
            _questManager.ProgressUserQuest(session, QuestType.SocialRespect);
            _achievementManager.ProgressAchievement(session, "ACH_RespectGiven", 1);
            _achievementManager.ProgressAchievement(targetUser.GetClient(), "ACH_RespectEarned", 1);

            //Take away from pet respect points, just in-case users abuse this..
            session.GetHabbo().GetStats().DailyPetRespectPoints -= 1;
            session.GetHabbo().GetStats().RespectGiven += 1;
            targetUser.GetClient().GetHabbo().GetStats().Respect += 1;

            //Apply the effect.
            thisUser.CarryItemId = 999999999;
            thisUser.CarryTimer = 5;

            //Send the magic out.
            if (room.RespectNotificationsEnabled)
                room.SendPacket(new RespectPetNotificationMessageComposer(targetUser.GetClient().GetHabbo(), targetUser));
            room.SendPacket(new CarryObjectComposer(thisUser.VirtualId, thisUser.CarryItemId));
            return Task.CompletedTask;
        }
        if (pet == null || pet.PetData == null || pet.RoomId != session.GetHabbo().CurrentRoomId)
            return Task.CompletedTask;
        session.GetHabbo().GetStats().DailyPetRespectPoints -= 1;
        _achievementManager.ProgressAchievement(session, "ACH_PetRespectGiver", 1);
        thisUser.CarryItemId = 999999999;
        thisUser.CarryTimer = 5;
        pet.PetData.OnRespect();
        room.SendPacket(new CarryObjectComposer(thisUser.VirtualId, thisUser.CarryItemId));
        return Task.CompletedTask;
    }
}