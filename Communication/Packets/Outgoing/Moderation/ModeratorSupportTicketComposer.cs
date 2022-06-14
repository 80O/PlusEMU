using Plus.HabboHotel.Moderation;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Moderation;

internal class ModeratorSupportTicketComposer : ServerPacket
{
    public ModeratorSupportTicketComposer(int id, ModerationTicket ticket)
        : base(ServerPacketHeader.ModeratorSupportTicketMessageComposer)
    {
        WriteInteger(ticket.Id); // Id
        WriteInteger(ticket.GetStatus(id)); // Tab ID
        WriteInteger(ticket.Type); // Type
        WriteInteger(ticket.Category); // Category
        WriteInteger(Convert.ToInt32((DateTime.Now - UnixTimestamp.FromUnixTimestamp(ticket.Timestamp)).TotalMilliseconds)); // This should fix the overflow?
        WriteInteger(ticket.Priority); // Priority
        WriteInteger(0); //??
        WriteInteger(ticket.Sender?.Id ?? 0); // Sender ID
        //base.WriteInteger(1);
        WriteString(ticket.Sender == null ? string.Empty : ticket.Sender.Username); // Sender Name
        WriteInteger(ticket.Reported?.Id ?? 0); // Reported ID
        WriteString(ticket.Reported == null ? string.Empty : ticket.Reported.Username); // Reported Name
        WriteInteger(ticket.Moderator?.Id ?? 0); // Moderator ID
        WriteString(ticket.Moderator == null ? string.Empty : ticket.Moderator.Username); // Mod Name
        WriteString(ticket.Issue); // Issue
        WriteInteger(ticket.Room?.Id ?? 0); // Room Id
        WriteInteger(0);
        {
            // push String
            // push Integer
            // push Integer
        }
    }

    /*public ModeratorSupportTicketComposer(SupportTicket Ticket)
         : base(ServerPacketHeader.ModeratorSupportTicketMessageComposer)
     {
         base.WriteInteger(Ticket.Id);
         base.WriteInteger(Ticket.TabId);
         base.WriteInteger(1); // Type
         base.WriteInteger(114); // Category
         base.WriteInteger(((int)PlusEnvironment.GetNow() - (int)Ticket.Timestamp) * 1000);
         base.WriteInteger(Ticket.Score);
         base.WriteInteger(0);
         base.WriteInteger(Ticket.SenderId);
         base.WriteString(Ticket.SenderName);
         base.WriteInteger(Ticket.ReportedId);
         base.WriteString(Ticket.ReportedName);
         base.WriteInteger((Ticket.Status == TicketStatus.PICKED) ? Ticket.ModeratorId : 0);
         base.WriteString(Ticket.ModName);
         base.WriteString(Ticket.Message);
         base.WriteInteger(0);//No idea?
         base.WriteInteger(0);//String, int, int - this is the "matched to" a string
         {
             base.WriteString("fresh-hotel.org");
             base.WriteInteger(-1);
             base.WriteInteger(-1);
         }
     }*/
}