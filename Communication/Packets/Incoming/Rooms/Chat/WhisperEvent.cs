using System;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.Core.Settings;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms.Chat.Filter;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat;

public class WhisperEvent : IPacketEvent
{
    private readonly IChatStyleManager _chatStyleManager;
    private readonly IChatlogManager _chatlogManager;
    private readonly IWordFilterManager _wordFilterManager;
    private readonly ICommandManager _commandManager;
    private readonly IModerationManager _moderationManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IQuestManager _questManager;

    public WhisperEvent(
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
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool") && room.CheckMute(session))
        {
            session.SendWhisper("Oops, you're currently muted.");
            return Task.CompletedTask;
        }
        if (UnixTimestamp.GetNow() < session.GetHabbo().FloodTime && session.GetHabbo().FloodTime != 0)
            return Task.CompletedTask;
        var @params = packet.PopString();
        var toUser = @params.Split(' ')[0];
        var message = @params.Substring(toUser.Length + 1);
        var colour = packet.PopInt();
        var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
        if (user == null)
            return Task.CompletedTask;
        var user2 = room.GetRoomUserManager().GetRoomUserByHabbo(toUser);
        if (user2 == null)
            return Task.CompletedTask;
        if (session.GetHabbo().TimeMuted > 0)
        {
            session.SendPacket(new MutedComposer(session.GetHabbo().TimeMuted));
            return Task.CompletedTask;
        }
        if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
            message = _wordFilterManager.CheckMessage(message);
        if (!_chatStyleManager.TryGetStyle(colour, out var style) ||
            style.RequiredRight.Length > 0 && !session.GetHabbo().GetPermissions().HasRight(style.RequiredRight))
            colour = 0;
        user.LastBubble = session.GetHabbo().CustomBubbleId == 0 ? colour : session.GetHabbo().CustomBubbleId;
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
        {
            if (user.IncrementAndCheckFlood(out var muteTime))
            {
                session.SendPacket(new FloodControlComposer(muteTime));
                return Task.CompletedTask;
            }
        }
        if (!user2.GetClient().GetHabbo().ReceiveWhispers && !session.GetHabbo().GetPermissions().HasRight("room_whisper_override"))
        {
            session.SendWhisper("Oops, this user has their whispers disabled!");
            return Task.CompletedTask;
        }
        _chatlogManager.StoreChatlog(new ChatlogEntry(session.GetHabbo().Id, room.Id, "<Whisper to " + toUser + ">: " + message, UnixTimestamp.GetNow(), session.GetHabbo(), room));
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
            session.SendPacket(new WhisperComposer(user.VirtualId, message, 0, user.LastBubble));
            return Task.CompletedTask;
        }
        _questManager.ProgressUserQuest(session, QuestType.SocialChat);
        user.UnIdle();
        user.GetClient().SendPacket(new WhisperComposer(user.VirtualId, message, 0, user.LastBubble));
        if (!user2.IsBot && user2.UserId != user.UserId)
        {
            if (!user2.GetClient().GetHabbo().GetIgnores().IgnoredUserIds().Contains(session.GetHabbo().Id))
                user2.GetClient().SendPacket(new WhisperComposer(user.VirtualId, message, 0, user.LastBubble));
        }
        var toNotify = room.GetRoomUserManager().GetRoomUserByRank(2);
        if (toNotify.Count > 0)
        {
            foreach (var notifiable in toNotify)
            {
                if (notifiable != null && notifiable.HabboId != user2.HabboId && notifiable.HabboId != user.HabboId)
                {
                    if (notifiable.GetClient() != null && notifiable.GetClient().GetHabbo() != null && !notifiable.GetClient().GetHabbo().IgnorePublicWhispers)
                        notifiable.GetClient().SendPacket(new WhisperComposer(user.VirtualId, "[Whisper to " + toUser + "] " + message, 0, user.LastBubble));
                }
            }
        }
        return Task.CompletedTask;
    }
}