using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class ModerateThreadMessageEvent : IPacketEvent
{
    public async Task Parse(GameClient Session, ClientPacket Packet)
    {
        var int1 = Packet.PopInt();
        var int2 = Packet.PopInt();
        var int3 = Packet.PopInt();

        GroupForum forum = PlusEnvironment.GetGame().GetGroupForumManager().GetForum(int1);

        if (forum is null)
        {
            Session.SendWhisper(("Uh-oh. The requested forum cannot be found."));
        }

        if (forum?.Settings.GetReasonForNot(Session, forum.Settings.WhoCanModerate) != "")
        {
            Session.SendWhisper(("You do not have the required permission to take action on this thread."));
        }

        GroupForumThread thread = forum?.GetThread(int2);

        if (thread is null)
        {
            Session.SendWhisper(("Unable to find the requested thread."));
        }

        thread.DeletedLevel = int3 / 10;
        thread.DeleterUserId = thread.DeletedLevel != 0 ? Session.GetHabbo().Id : 0;
        await thread.Save();

        Session.SendPacket(new ThreadsListDataComposer(forum, Session));

        if (thread.DeletedLevel != 0)
            Session.SendWhisper("Successfully removed thread from public view.");
        else
            Session.SendWhisper("Thread has been successfully restored to public view.");
    }
}
