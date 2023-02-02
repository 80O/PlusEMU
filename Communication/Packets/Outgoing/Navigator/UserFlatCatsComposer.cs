using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Outgoing.Navigator;

public class UserFlatCatsComposer : IServerPacket
{
    private readonly IReadOnlyCollection<SearchResultList> _categories;
    private readonly int _rank;

    public uint MessageId => ServerPacketHeader.UserFlatCatsComposer;

    public UserFlatCatsComposer(IReadOnlyCollection<SearchResultList> categories, int rank)
    {
        _categories = categories;
        _rank = rank;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_categories.Count);
        foreach (var category in _categories)
        {
            packet.WriteInteger(category.Id);
            packet.WriteString(category.PublicName);
            packet.WriteBoolean(category.RequiredRank <= _rank);
            packet.WriteBoolean(false);
            packet.WriteString(string.Empty);
            packet.WriteString(string.Empty);
            packet.WriteBoolean(false);
        }
    }
}