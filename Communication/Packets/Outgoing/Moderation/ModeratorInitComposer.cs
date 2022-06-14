using Plus.HabboHotel.Moderation;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Moderation;

internal class ModeratorInitComposer : ServerPacket
{
    public ModeratorInitComposer(ICollection<string> userPresets, ICollection<string> roomPresets, ICollection<ModerationTicket> tickets)
        : base(ServerPacketHeader.ModeratorInitMessageComposer)
    {
        WriteInteger(tickets.Count);
        foreach (var ticket in tickets)
        {
            WriteInteger(ticket.Id); // Id
            WriteInteger(ticket.GetStatus(Id)); // Tab ID
            WriteInteger(ticket.Type); // Type
            WriteInteger(ticket.Category); // Category
            WriteInteger(Convert.ToInt32((DateTime.Now - UnixTimestamp.FromUnixTimestamp(ticket.Timestamp)).TotalMilliseconds)); // This should fix the overflow?
            WriteInteger(ticket.Priority); // Priority
            WriteInteger(ticket.Sender?.Id ?? 0); // Sender ID
            WriteInteger(1);
            WriteString(ticket.Sender == null ? string.Empty : ticket.Sender.Username); // Sender Name
            WriteInteger(ticket.Reported?.Id ?? 0); // Reported ID
            WriteString(ticket.Reported == null ? string.Empty : ticket.Reported.Username); // Reported Name
            WriteInteger(ticket.Moderator?.Id ?? 0); // Moderator ID
            WriteString(ticket.Moderator == null ? string.Empty : ticket.Moderator.Username); // Mod Name
            WriteString(ticket.Issue); // Issue
            WriteInteger(ticket.Room?.Id ?? 0); // Room Id
            WriteInteger(0); //LOOP
        }
        WriteInteger(userPresets.Count);
        foreach (var pre in userPresets) WriteString(pre);
        /*base.WriteInteger(UserActionPresets.Count);
        foreach (KeyValuePair<string, List<ModerationPresetActionMessages>> Cat in UserActionPresets.ToList())
        {
            base.WriteString(Cat.Key);
            base.WriteBoolean(true);
            base.WriteInteger(Cat.Value.Count);
            foreach (ModerationPresetActionMessages Preset in Cat.Value.ToList())
            {
                base.WriteString(Preset.Caption);
                base.WriteString(Preset.MessageText);
                base.WriteInteger(Preset.BanTime); // Account Ban Hours
                base.WriteInteger(Preset.IPBanTime); // IP Ban Hours
                base.WriteInteger(Preset.MuteTime); // Mute in Hours
                base.WriteInteger(0);//Trading lock duration
                base.WriteString(Preset.Notice + "\n\nPlease Note: Avatar ban is an IP ban!");
                base.WriteBoolean(false);//Show HabboWay
            }
        }*/

        // TODO: Figure out
        WriteInteger(0);
        {
            //Loop a string.
        }
        WriteBoolean(true); // Ticket right
        WriteBoolean(true); // Chatlogs
        WriteBoolean(true); // User actions alert etc
        WriteBoolean(true); // Kick users
        WriteBoolean(true); // Ban users
        WriteBoolean(true); // Caution etc
        WriteBoolean(true); // Love you, Tom
        WriteInteger(roomPresets.Count);
        foreach (var pre in roomPresets) WriteString(pre);
    }
}