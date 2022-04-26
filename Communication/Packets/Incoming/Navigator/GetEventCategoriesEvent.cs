using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Incoming.Navigator;

internal class GetEventCategoriesEvent : IPacketEvent
{
    private readonly INavigatorManager _navigatorManager;

    public GetEventCategoriesEvent(INavigatorManager navigatorManager)
    {
        _navigatorManager = navigatorManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var categories = _navigatorManager.GetEventCategories();
        session.SendPacket(new NavigatorFlatCatsComposer(categories));
        return Task.CompletedTask;
    }
}