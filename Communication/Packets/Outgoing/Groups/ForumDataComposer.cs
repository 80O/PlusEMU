using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups;

internal class ForumDataComposer : ServerPacket
{
    public ForumDataComposer(GroupForum forum, GameClient session)
            : base(ServerPacketHeader.ForumDataMessageComposer)
    {
        base.WriteInteger(forum.Id);
        base.WriteString(forum.Group.Name);
        base.WriteString(forum.Group.Description);
        base.WriteString(forum.Group.Badge);

        base.WriteInteger(forum.Threads.Count);
        base.WriteInteger(0); //Last Author ID
        base.WriteInteger(0); //Score ?
        base.WriteInteger(0); //Last Thread Mark
        base.WriteInteger(0);
        base.WriteInteger(0);
        base.WriteString("not_member");
        base.WriteInteger(0);

        base.WriteInteger(forum.Settings.WhoCanRead); 
        base.WriteInteger(forum.Settings.WhoCanPost);
        base.WriteInteger(forum.Settings.WhoCanInitDiscussions);
        base.WriteInteger(forum.Settings.WhoCanModerate);

        base.WriteString(forum.Settings.GetReasonForNot(session, forum.Settings.WhoCanRead));
        base.WriteString(forum.Settings.GetReasonForNot(session, forum.Settings.WhoCanPost));
        base.WriteString(forum.Settings.GetReasonForNot(session, forum.Settings.WhoCanInitDiscussions));
        base.WriteString(forum.Settings.GetReasonForNot(session, forum.Settings.WhoCanModerate));
        base.WriteString("");

        base.WriteBoolean(forum.Group.CreatorId == session.GetHabbo().Id); // Is Owner
        base.WriteBoolean(forum.Group.IsAdmin(session.GetHabbo().Id) && forum.Settings.GetReasonForNot(session, forum.Settings.WhoCanModerate) == ""); // Is admin
    }
}
