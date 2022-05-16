using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups;

internal class ForumDataComposer : ServerPacket
{
    public ForumDataComposer(GroupForum Forum, GameClient Session)
            : base(ServerPacketHeader.ForumDataMessageComposer)
    {
        base.WriteInteger(Forum.Id);
        base.WriteString(Forum.Group.Name);
        base.WriteString(Forum.Group.Description);
        base.WriteString(Forum.Group.Badge);

        base.WriteInteger(Forum.Threads.Count);
        base.WriteInteger(0); //Last Author ID
        base.WriteInteger(0); //Score ?
        base.WriteInteger(0); //Last Thread Mark
        base.WriteInteger(0);
        base.WriteInteger(0);
        base.WriteString("not_member");
        base.WriteInteger(0);

        base.WriteInteger(Forum.Settings.WhoCanRead); 
        base.WriteInteger(Forum.Settings.WhoCanPost);
        base.WriteInteger(Forum.Settings.WhoCanInitDiscussions);
        base.WriteInteger(Forum.Settings.WhoCanModerate);

        base.WriteString(Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanRead));
        base.WriteString(Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanPost));
        base.WriteString(Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanInitDiscussions));
        base.WriteString(Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanModerate));
        base.WriteString("");

        base.WriteBoolean(Forum.Group.CreatorId == Session.GetHabbo().Id); // Is Owner
        base.WriteBoolean(Forum.Group.IsAdmin(Session.GetHabbo().Id) && Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanModerate) == ""); // Is admin
    }
}
