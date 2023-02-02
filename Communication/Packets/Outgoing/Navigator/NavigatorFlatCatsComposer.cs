using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Outgoing.Navigator;

public class NavigatorFlatCatsComposer : IServerPacket
{
    private readonly IReadOnlyCollection<SearchResultList> _categories;
    public uint MessageId => ServerPacketHeader.NavigatorFlatCatsComposer;

    public NavigatorFlatCatsComposer(IReadOnlyCollection<SearchResultList> categories)
    {
        _categories = categories;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_categories.Count);
        foreach (var category in _categories.ToList())
        {
            packet.WriteInteger(category.Id);
            packet.WriteString(category.PublicName);
            packet.WriteBoolean(true); // TODO
        }
    }
}