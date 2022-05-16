using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Groups;


internal class ForumsListDataComposer : ServerPacket
{
    public ForumsListDataComposer(ICollection<GroupForum> forums, GameClient session, int viewOrder = 0, int startIndex = 0, int maxLength = 20)
            : base(ServerPacketHeader.ForumsListDataMessageComposer)
    {
        base.WriteInteger(viewOrder);
        base.WriteInteger(startIndex);
        base.WriteInteger(startIndex);

        base.WriteInteger(forums.Count);

        foreach (GroupForum forum in forums)
        {
            var lastPost = forum.GetLastPost();
            var isn = lastPost == null;
            base.WriteInteger(forum.Id);
            base.WriteString(forum.Name);
            base.WriteString(forum.Description);
            base.WriteString(forum.Group.Badge);
            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(forum.MessagesCount);
            base.WriteInteger(forum.UnreadMessages(session.GetHabbo().Id));
            base.WriteInteger(0);
            base.WriteInteger(!isn ? lastPost.GetAuthor().Id : 0);
            base.WriteString(!isn ? lastPost.GetAuthor().Username : "");
            base.WriteInteger(!isn ? (int)PlusEnvironment.GetUnixTimestamp() - lastPost.Timestamp : 0);
        }
    }
}
