using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Outgoing.Navigator;

internal class NavigatorFlatCatsComposer : ServerPacket
{
    public NavigatorFlatCatsComposer(ICollection<SearchResultList> categories)
        : base(ServerPacketHeader.NavigatorFlatCatsMessageComposer)
    {
        WriteInteger(categories.Count);
        foreach (var category in categories.ToList())
        {
            WriteInteger(category.Id);
            WriteString(category.PublicName);
            WriteBoolean(true); // TODO
        }
    }
}