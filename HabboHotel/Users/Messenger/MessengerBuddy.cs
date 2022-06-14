using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Users.Messenger;

public class MessengerBuddy
{
    [Obsolete("Should be removed")]
    public GameClient? Client;
    private Habbo? _habbo;

    public Habbo? Habbo
    {
        get => _habbo;
        set
        {
            _habbo = value;
            if (_habbo != null)
            {
                Look = _habbo.Look;
                Motto = _habbo.Motto;
                Gender = _habbo.Gender.Equals("M", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            }
        }
    }

    public bool AppearOffline => _habbo == null;

    public bool HideInRoom => _habbo?.AllowUserFollowing ?? true;
    public int LastOnline { get; set; }
    public string Look { get; set; } = string.Empty;
    public string Motto { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Relationship { get; set; }
    public int Gender { get; set; }
    public int Id { get; set; }

    public bool InRoom => CurrentRoom != null;

    public Room? CurrentRoom { get; set; }

    public void Serialize(ServerPacket message)
    {
        message.WriteInteger(Id);
        message.WriteString(Username);
        message.WriteInteger(Gender);
        message.WriteBoolean(!AppearOffline);
        message.WriteBoolean(!HideInRoom);
        message.WriteString(Look);
        message.WriteInteger(0); // categoryid
        message.WriteString(Motto);
        message.WriteString(string.Empty); // Facebook username
        message.WriteString(string.Empty);
        message.WriteBoolean(true); // Allows offline messaging
        message.WriteBoolean(false); // ?
        message.WriteBoolean(false); // Uses phone
        message.WriteShort(Relationship);
    }
}