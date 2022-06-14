using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Outgoing.Navigator.New;

internal class NavigatorSearchResultSetComposer : ServerPacket
{
    public NavigatorSearchResultSetComposer(string category, string data,
        ICollection<SearchResultList> searchResultLists, GameClient session, int goBack = 1, int fetchLimit = 12)
        : base(ServerPacketHeader.NavigatorSearchResultSetMessageComposer)
    {
        WriteString(category); //Search code.
        WriteString(data); //Text?
        WriteInteger(searchResultLists.Count); //Count
        foreach (var searchResult in searchResultLists.ToList())
        {
            WriteString(searchResult.CategoryIdentifier);
            WriteString(searchResult.PublicName);
            WriteInteger(NavigatorSearchAllowanceUtility.GetIntegerValue(searchResult.SearchAllowance) != 0
                ? goBack
                : NavigatorSearchAllowanceUtility.GetIntegerValue(searchResult
                    .SearchAllowance)); //0 = nothing, 1 = show more, 2 = back Action allowed.
            WriteBoolean(false); //True = minimized, false = open.
            WriteInteger(searchResult.ViewMode == NavigatorViewMode.Regular ? 0 :
                searchResult.ViewMode == NavigatorViewMode.Thumbnail ? 1 :
                0); //View mode, 0 = tiny/regular, 1 = thumbnail
            NavigatorHandler.Search(this, searchResult, data, session, fetchLimit);
        }
    }
}