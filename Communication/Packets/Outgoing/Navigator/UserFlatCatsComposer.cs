using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Outgoing.Navigator;

internal class UserFlatCatsComposer : ServerPacket
{
    public UserFlatCatsComposer(ICollection<SearchResultList> categories, int rank)
        : base(ServerPacketHeader.UserFlatCatsMessageComposer)
    {
        WriteInteger(categories.Count);
        foreach (var category in categories)
        {
            WriteInteger(category.Id);
            WriteString(category.PublicName);
            WriteBoolean(category.RequiredRank <= rank);
            WriteBoolean(false);
            WriteString(string.Empty);
            WriteString(string.Empty);
            WriteBoolean(false);
        }
    }
}