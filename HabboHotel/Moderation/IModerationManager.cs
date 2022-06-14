namespace Plus.HabboHotel.Moderation;

public interface IModerationManager
{
    ICollection<string> UserMessagePresets { get; }
    ICollection<string> RoomMessagePresets { get; }
    ICollection<ModerationTicket> GetTickets { get; }
    Dictionary<string, List<ModerationPresetActions>> UserActionPresets { get; }
    void Init();
    void ReCacheBans();
    void BanUser(string mod, ModerationBanType type, string banValue, string reason, double expireTimestamp);
    bool TryAddTicket(ModerationTicket ticket);
    bool TryGetTicket(int ticketId, out ModerationTicket ticket);
    bool UserHasTickets(int userId);
    ModerationTicket GetTicketBySenderId(int userId);

    /// <summary>
    /// Runs a quick check to see if a ban record is cached in the server.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ban"></param>
    /// <returns></returns>
    bool IsBanned(string key, out ModerationBan ban);

    /// <summary>
    /// Run a quick database check to see if this ban exists in the database.
    /// </summary>
    /// <param name="machineId">The value of the ban.</param>
    /// <returns></returns>
    bool HasMachineBanCheck(string machineId);

    /// <summary>
    /// Run a quick database check to see if this ban exists in the database.
    /// </summary>
    /// <param name="username">The value of the ban.</param>
    /// <returns></returns>
    bool UsernameBanCheck(string username);

    /// <summary>
    /// Remove a ban from the cache based on a given value.
    /// </summary>
    /// <param name="value"></param>
    void RemoveBan(string value);
}