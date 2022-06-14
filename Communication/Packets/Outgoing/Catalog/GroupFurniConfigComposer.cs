using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Catalog;

internal class GroupFurniConfigComposer : ServerPacket
{
    public GroupFurniConfigComposer(ICollection<Group> groups)
        : base(ServerPacketHeader.GroupFurniConfigMessageComposer)
    {
        WriteInteger(groups.Count);
        foreach (var group in groups)
        {
            WriteInteger(group.Id);
            WriteString(group.Name);
            WriteString(group.Badge);
            WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour1, true));
            WriteString(PlusEnvironment.GetGame().GetGroupManager().GetColourCode(group.Colour2, false));
            WriteBoolean(false);
            WriteInteger(group.CreatorId);
            WriteBoolean(group.ForumEnabled);
        }
    }
}