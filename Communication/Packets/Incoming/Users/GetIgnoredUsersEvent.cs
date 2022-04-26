using System.Collections.Generic;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users;

internal class GetIgnoredUsersEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        var ignoredUsers = new List<string>();
        foreach (var userId in new List<int>(session.GetHabbo().GetIgnores().IgnoredUserIds()))
        {
            var player = PlusEnvironment.GetHabboById(userId);
            if (player != null)
            {
                if (!ignoredUsers.Contains(player.Username))
                    ignoredUsers.Add(player.Username);
            }
        }
        session.SendPacket(new IgnoredUsersComposer(ignoredUsers));
        return Task.CompletedTask;
    }
}