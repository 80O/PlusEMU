using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Moderation;

public class ModerationTicket
{
    public List<string> ReportedChats;

    public ModerationTicket(int id, int type, int category, double timestamp, int priority, Habbo sender, Habbo reported, string issue, RoomData room, List<string> reportedChats)
    {
        Id = id;
        Type = type;
        Category = category;
        Timestamp = timestamp;
        Priority = priority;
        Sender = sender;
        Reported = reported;
        Moderator = null;
        Issue = issue;
        Room = room;
        Answered = false;
        ReportedChats = reportedChats;
    }

    public int Id { get; set; }
    public int Type { get; set; }
    public int Category { get; set; }
    public double Timestamp { get; set; }
    public int Priority { get; set; }
    public bool Answered { get; set; }
    public Habbo Sender { get; set; }
    public Habbo Reported { get; set; }
    public Habbo Moderator { get; set; }
    public string Issue { get; set; }
    public RoomData Room { get; set; }

    public int GetStatus(int id)
    {
        if (Moderator == null)
            return 1;
        if (Moderator.Id == id && !Answered)
            return 2;
        if (Answered)
            return 3;
        return 3;
    }
}