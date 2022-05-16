using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups;

internal class UpdateThreadEvent : IPacketEvent
{
    private readonly IGroupForumManager _groupForumManager;
    public UpdateThreadEvent(IGroupForumManager groupForumManager) => _groupForumManager = groupForumManager;
    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var forumId = packet.PopInt();
        var threadId = packet.PopInt();
        var pinned = packet.PopBoolean();
        var locked = packet.PopBoolean();

        GroupForum forum = _groupForumManager.GetForum(forumId);
        GroupForumThread thread = forum.GetThread(threadId);

        if (forum.Settings.GetReasonForNot(session, forum.Settings.WhoCanModerate) != "")
        {
            session.SendWhisper(("Oops! You are not admin on this Thread!"));
        }

        bool isPining = thread.pinned != pinned,
            isLocking = thread.locked != locked;

        thread.pinned = pinned;
        thread.locked = locked;
        
        await thread.Save();

        session.SendPacket(new Outgoing.Groups.ThreadUpdatedComposer(session, thread));

        if (isPining)
            if (pinned)
                session.SendWhisper(("Thread has been successfully pinned."));
            else
                session.SendWhisper(("Thread has been successfully unpinned."));

        if (isLocking)
            if (locked)
                session.SendWhisper(("Thread has been successfully locked."));
            else
                session.SendWhisper(("Thread has been successfully re-opened."));
    }
}