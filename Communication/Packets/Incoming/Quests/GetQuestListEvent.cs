using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Incoming.Quests;

public class GetQuestListEvent : IPacketEvent
{
    private readonly IQuestManager _questManager;

    public GetQuestListEvent(IQuestManager questManager)
    {
        _questManager = questManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        _questManager.GetList(session, null);
        return Task.CompletedTask;
    }
}