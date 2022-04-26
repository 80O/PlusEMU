using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Incoming.Navigator;

public class GetUserFlatCatsEvent : IPacketEvent
{
    private readonly INavigatorManager _navigatorManager;

    public GetUserFlatCatsEvent(INavigatorManager navigatorManager)
    {
        _navigatorManager = navigatorManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var categories = _navigatorManager.GetFlatCategories();
        session.SendPacket(new UserFlatCatsComposer(categories, session.GetHabbo().Rank));
        return Task.CompletedTask;
    }
}