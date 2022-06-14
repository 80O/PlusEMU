using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Users;

internal class ProfileInformationComposer : ServerPacket
{
    public ProfileInformationComposer(Habbo habbo, GameClient session, List<Group> groups, int friendCount)
        : base(ServerPacketHeader.ProfileInformationMessageComposer)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(habbo.AccountCreated);
        WriteInteger(habbo.Id);
        WriteString(habbo.Username);
        WriteString(habbo.Look);
        WriteString(habbo.Motto);
        WriteString(origin.ToString("dd/MM/yyyy"));
        WriteInteger(habbo.GetStats().AchievementPoints);
        WriteInteger(friendCount); // Friend Count
        WriteBoolean(habbo.Id != session.GetHabbo().Id && session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id)); //  Is friend
        WriteBoolean(habbo.Id != session.GetHabbo().Id && !session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id) &&
                     session.GetHabbo().GetMessenger().OutstandingFriendRequests.Contains(habbo.Id)); // Sent friend request
        WriteBoolean(PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(habbo.Id) != null);
        WriteInteger(groups.Count);
        foreach (var group in groups)
        {
            WriteInteger(group.Id);
            WriteString(group.Name);
            WriteString(group.Badge);
            WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour1, true));
            WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour2, false));
            WriteBoolean(habbo.GetStats().FavouriteGroupId == group.Id); // todo favs
            WriteInteger(0); //what the fuck
            WriteBoolean(group?.ForumEnabled ?? true); //HabboTalk
        }
        WriteInteger(Convert.ToInt32(UnixTimestamp.GetNow() - habbo.LastOnline)); // Last online
        WriteBoolean(true); // Show the profile
    }
}