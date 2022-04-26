using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Navigator.New;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Incoming.Navigator;

internal class InitializeNewNavigatorEvent : IPacketEvent
{
    private readonly INavigatorManager _navigatorManager;

    public InitializeNewNavigatorEvent(INavigatorManager navigatorManager)
    {
        _navigatorManager = navigatorManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var topLevelItems = _navigatorManager.GetTopLevelItems();
        session.SendPacket(new NavigatorMetaDataParserComposer(topLevelItems));
        session.SendPacket(new NavigatorLiftedRoomsComposer());
        session.SendPacket(new NavigatorCollapsedCategoriesComposer());
        session.SendPacket(new NavigatorPreferencesComposer());
        return Task.CompletedTask;
    }
}