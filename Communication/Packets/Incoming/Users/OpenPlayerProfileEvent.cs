using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Users;

internal class OpenPlayerProfileEvent : IPacketEvent
{
    private readonly IGroupManager _groupManager;
    private readonly IDatabase _database;

    public OpenPlayerProfileEvent(IGroupManager groupManager, IDatabase database)
    {
        _groupManager = groupManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();
        packet.PopBoolean(); //IsMe?
        var targetData = PlusEnvironment.GetHabboById(userId);
        if (targetData == null)
        {
            session.SendNotification("An error occured whilst finding that user's profile.");
            return Task.CompletedTask;
        }
        var groups = _groupManager.GetGroupsForUser(targetData.Id);
        int friendCount;
        using (var connection = _database.Connection())
        {
            friendCount = connection.ExecuteScalar<int>("SELECT count(0) FROM messenger_friendships WHERE user_one_id = @userid OR user_two_id = @userid", new { userid = userId });
        }
        session.SendPacket(new ProfileInformationComposer(targetData, session, groups, friendCount));
        return Task.CompletedTask;
    }
}