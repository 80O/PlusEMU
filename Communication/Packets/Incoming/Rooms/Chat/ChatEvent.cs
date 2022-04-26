using System;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Core.Settings;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Filter;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat;

public class ChatEvent : IPacketEvent
{
    private readonly IChatStyleManager _chatStyleManager;
    private readonly IChatlogManager _chatlogManager;
    private readonly IWordFilterManager _wordFilterManager;
    private readonly ICommandManager _commandManager;
    private readonly IModerationManager _moderationManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IQuestManager _questManager;

    public ChatEvent(
        IChatStyleManager chatStyleManager,
        IChatlogManager chatlogManager,
        IWordFilterManager wordFilterManager,
        ICommandManager commandManager,
        IModerationManager moderationManager,
        ISettingsManager settingsManager,
        IQuestManager questManager)
    {
        _chatStyleManager = chatStyleManager;
        _chatlogManager = chatlogManager;
        _wordFilterManager = wordFilterManager;
        _commandManager = commandManager;
        _moderationManager = moderationManager;
        _settingsManager = settingsManager;
        _questManager = questManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetHabbo().InRoom)
            return Task.CompletedTask;
        var room = session.GetHabbo().CurrentRoom;
        if (room == null)
            return Task.CompletedTask;
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return Task.CompletedTask;
        var message = StringCharFilter.Escape(packet.PopString());
        if (message.Length > 100)
            message = message.Substring(0, 100);
        var colour = packet.PopInt();
        if (!_chatStyleManager.TryGetStyle(colour, out var style) ||
            style.RequiredRight.Length > 0 && !session.GetHabbo().GetPermissions().HasRight(style.RequiredRight))
            colour = 0;
        user.UnIdle();
        if (UnixTimestamp.GetNow() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
            return Task.CompletedTask;
        if (session.GetHabbo().TimeMuted > 0)
        {
            session.SendPacket(new MutedComposer(session.GetHabbo().TimeMuted));
            return Task.CompletedTask;
        }
        if (!session.GetHabbo().GetPermissions().HasRight("room_ignore_mute") && room.CheckMute(session))
        {
            session.SendWhisper("Oops, you're currently muted.");
            return Task.CompletedTask;
        }
        user.LastBubble = session.GetHabbo().CustomBubbleId == 0 ? colour : session.GetHabbo().CustomBubbleId;
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
        {
            if (user.IncrementAndCheckFlood(out var muteTime))
            {
                session.SendPacket(new FloodControlComposer(muteTime));
                return Task.CompletedTask;
            }
        }
        
        _chatlogManager.StoreChatlog(new ChatlogEntry(session.GetHabbo().Id, room.Id, message, UnixTimestamp.GetNow(), session.GetHabbo(), room));
        
        if (message.StartsWith(":", StringComparison.CurrentCulture) && _commandManager.Parse(session, message))
            return Task.CompletedTask;
        if (_wordFilterManager.CheckBannedWords(message))
        {
            session.GetHabbo().BannedPhraseCount++;
            if (session.GetHabbo().BannedPhraseCount >= Convert.ToInt32(_settingsManager.TryGetValue("room.chat.filter.banned_phrases.chances")))
            {
                _moderationManager.BanUser("System", ModerationBanType.Username, session.GetHabbo().Username, "Spamming banned phrases (" + message + ")",
                    UnixTimestamp.GetNow() + 78892200);
                session.Disconnect();
                return Task.CompletedTask;
            }
            session.SendPacket(new ChatComposer(user.VirtualId, message, 0, colour));
            return Task.CompletedTask;
        }
        if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
            message = _wordFilterManager.CheckMessage(message);
        _questManager.ProgressUserQuest(session, QuestType.SocialChat);
        user.OnChat(user.LastBubble, message, false);
        return Task.CompletedTask;
    }
}