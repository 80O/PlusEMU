using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups;

public class GroupMembersComposer : IServerPacket
{
    private readonly Group _group;
    private readonly ICollection<CachedUser> _members;
    private readonly int _membersCount;
    private readonly int _page;
    private readonly bool _admin;
    private readonly int _reqType;
    private readonly string _searchVal;

    public uint MessageId => ServerPacketHeader.GroupMembersComposer;

    public GroupMembersComposer(Group group, ICollection<CachedUser> members, int membersCount, int page, bool admin, int reqType, string searchVal)
    {
        _group = group;
        _members = members;
        _membersCount = membersCount;
        _page = page;
        _admin = admin;
        _reqType = reqType;
        _searchVal = searchVal;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_group.Id);
        packet.WriteString(_group.Name);
        packet.WriteUInteger(_group.RoomId);
        packet.WriteString(_group.Badge);
        packet.WriteInteger(_membersCount);
        packet.WriteInteger(_members.Count);
        if (_membersCount > 0)
        {
            foreach (var data in _members)
            {
                packet.WriteInteger(_group.CreatorId == data.Id ? 0 : _group.IsAdmin(data.Id) ? 1 : _group.IsMember(data.Id) ? 2 : 3);
                packet.WriteInteger(data.Id);
                packet.WriteString(data.Username);
                packet.WriteString(data.Look);
                packet.WriteString(string.Empty);
            }
        }
        packet.WriteBoolean(_admin);
        packet.WriteInteger(14);
        packet.WriteInteger(_page);
        packet.WriteInteger(_reqType);
        packet.WriteString(_searchVal);
    }
}