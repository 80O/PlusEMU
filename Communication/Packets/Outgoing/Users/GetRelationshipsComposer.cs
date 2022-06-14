using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Users;

internal class GetRelationshipsComposer : ServerPacket
{
    public GetRelationshipsComposer(Habbo habbo) : base(ServerPacketHeader.GetRelationshipsMessageComposer)
    {
        WriteInteger(habbo.Id);
        var relationships = habbo.GetMessenger().Friends.Values.Where(f => f.Relationship > 0).GroupBy(f => f.Relationship).ToDictionary(f => f.Key, f => (f.First(), f.Count()));
        WriteInteger(relationships.Count); // Count
        foreach (var (type, (friend, count)) in relationships)
        {
            WriteInteger(type);
            WriteInteger(count);
            WriteInteger(friend.Id); // Their ID
            WriteString(friend.Username);
            WriteString(friend.Look);
        }
    }
}