using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateThreadEvent : IPacketEvent
{
    public async Task Parse(GameClient Session, ClientPacket Packet)
    {
        var ForumID = Packet.PopInt();
        var ThreadID = Packet.PopInt();
        var Pinned = Packet.PopBoolean();
        var Locked = Packet.PopBoolean();

        GroupForum forum = PlusEnvironment.GetGame().GetGroupForumManager().GetForum(ForumID);
        GroupForumThread thread = forum.GetThread(ThreadID);

        if (forum.Settings.GetReasonForNot(Session, forum.Settings.WhoCanModerate) != "")
        {
            Session.SendWhisper(("Oops! You are not admin on this Thread!"));
        }

        bool isPining = thread.Pinned != Pinned,
            isLocking = thread.Locked != Locked;

        thread.Pinned = Pinned;
        thread.Locked = Locked;
        
        await thread.Save();

        Session.SendPacket(new Outgoing.Groups.ThreadUpdatedComposer(Session, thread));

        if (isPining)
            if (Pinned)
                Session.SendWhisper(("Thread has been successfully pinned."));
            else
                Session.SendWhisper(("Thread has been successfully unpinned."));

        if (isLocking)
            if (Locked)
                Session.SendWhisper(("Thread has been successfully locked."));
            else
                Session.SendWhisper(("Thread has been successfully re-opened."));
    }
}