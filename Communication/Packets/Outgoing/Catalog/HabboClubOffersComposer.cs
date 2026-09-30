using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Catalog;

namespace Plus.Communication.Packets.Outgoing.Catalog;

public sealed class HabboClubOffersComposer : IServerPacket
{
    private readonly IReadOnlyCollection<CatalogClubOffer> _offers;
    private readonly int _source;

    public HabboClubOffersComposer(IReadOnlyCollection<CatalogClubOffer> offers, int source)
    {
        _offers = offers;
        _source = source;
    }

    public uint MessageId => ServerPacketHeader.HabboClubOffersComposer;

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_offers.Count);
        foreach (var offer in _offers)
        {
            packet.WriteInteger(offer.Id);
            packet.WriteString(offer.Name);
            packet.WriteBoolean(false);
            packet.WriteInteger(offer.Credits);
            packet.WriteInteger(offer.Points);
            packet.WriteInteger(offer.PointsType);
            packet.WriteBoolean(offer.IsVip);
            packet.WriteInteger(offer.Months);
            packet.WriteInteger(offer.Days);
            packet.WriteBoolean(offer.Deal);
            packet.WriteInteger(0);
            packet.WriteInteger(0);
            packet.WriteInteger(0);
            packet.WriteInteger(0);
        }

        packet.WriteInteger(_source);
    }
}
