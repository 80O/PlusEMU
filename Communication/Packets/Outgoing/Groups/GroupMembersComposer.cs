using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups;

internal class GroupMembersComposer : ServerPacket
{
    public GroupMembersComposer(Group group, ICollection<UserCache> members, int membersCount, int page, bool admin, int reqType, string searchVal)
        : base(ServerPacketHeader.GroupMembersMessageComposer)
    {
        WriteInteger(group.Id);
        WriteString(group.Name);
        WriteInteger(group.RoomId);
        WriteString(group.Badge);
        WriteInteger(membersCount);
        WriteInteger(members.Count);
        if (membersCount > 0)
        {
            foreach (var data in members)
            {
                WriteInteger(group.CreatorId == data.Id ? 0 : group.IsAdmin(data.Id) ? 1 : group.IsMember(data.Id) ? 2 : 3);
                WriteInteger(data.Id);
                WriteString(data.Username);
                WriteString(data.Look);
                WriteString(string.Empty);
            }
        }
        WriteBoolean(admin);
        WriteInteger(14);
        WriteInteger(page);
        WriteInteger(reqType);
        WriteString(searchVal);
    }
}