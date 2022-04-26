using System.Collections.Generic;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Navigator.New;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Incoming.Navigator;

internal class NavigatorSearchEvent : IPacketEvent
{
    private readonly INavigatorManager _navigatorManager;

    public NavigatorSearchEvent(INavigatorManager navigatorManager)
    {
        _navigatorManager = navigatorManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var category = packet.PopString();
        var search = packet.PopString();
        ICollection<SearchResultList> categories = new List<SearchResultList>();
        if (!string.IsNullOrEmpty(search))
        {
            if (_navigatorManager.TryGetSearchResultList(0, out var queryResult)) categories.Add(queryResult);
        }
        else
        {
            categories = _navigatorManager.GetCategorysForSearch(category);
            if (categories.Count == 0)
            {
                //Are we going in deep?!
                categories = _navigatorManager.GetResultByIdentifier(category);
                if (categories.Count > 0)
                {
                    session.SendPacket(new NavigatorSearchResultSetComposer(category, search, categories, session, 2, 100));
                    return Task.CompletedTask;
                }
            }
        }
        session.SendPacket(new NavigatorSearchResultSetComposer(category, search, categories, session));
        return Task.CompletedTask;
    }
}