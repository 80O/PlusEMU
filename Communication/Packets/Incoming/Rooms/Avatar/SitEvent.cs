using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar;

internal class SitEvent : IPacketEvent
{
    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var user = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return Task.CompletedTask;
        if (user.Statusses.ContainsKey("lie") || user.IsLying || user.RidingHorse || user.IsWalking) return Task.CompletedTask;
        if (!user.Statusses.ContainsKey("sit"))
        {
            if (user.RotBody % 2 == 0)
            {
                try
                {
                    user.Statusses.Add("sit", "1.0");
                    user.Z -= 0.35;
                    user.IsSitting = true;
                    user.UpdateNeeded = true;
                }
                catch
                {
                    //ignored
                }
            }
            else
            {
                user.RotBody--;
                user.Statusses.Add("sit", "1.0");
                user.Z -= 0.35;
                user.IsSitting = true;
                user.UpdateNeeded = true;
            }
        }
        else if (user.IsSitting)
        {
            user.Z += 0.35;
            user.Statusses.Remove("sit");
            user.Statusses.Remove("1.0");
            user.IsSitting = false;
            user.UpdateNeeded = true;
        }
        return Task.CompletedTask;
    }
}