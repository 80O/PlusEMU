using Microsoft.Extensions.Logging;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Emotions;
using Plus.HabboHotel.Rooms.Chat.Filter;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Rooms.Chat.Pets.Commands;
using Plus.HabboHotel.Rooms.Chat.Pets.Locale;
using Plus.HabboHotel.Rooms.Chat.Styles;

namespace Plus.HabboHotel.Rooms.Chat;

public sealed class ChatManager : IChatManager
{
    private readonly ILogger<ChatManager> _logger;

    /// <summary>
    /// Chat styles.
    /// </summary>
    private readonly IChatStyleManager _chatStyles;

    /// <summary>
    /// Commands.
    /// </summary>
    private readonly ICommandManager _commands;

    /// <summary>
    /// Chat Emoticons.
    /// </summary>
    private readonly IChatEmotionsManager _emotions;

    /// <summary>
    /// Filter Manager.
    /// </summary>
    private readonly IWordFilterManager _filter;

    /// <summary>
    /// Chatlog Manager
    /// </summary>
    private readonly IChatlogManager _logs;

    /// <summary>
    /// Pet Commands.
    /// </summary>
    private readonly IPetCommandManager _petCommands;

    /// <summary>
    /// Pet Locale.
    /// </summary>
    private readonly IPetLocale _petLocale;

    /// <summary>
    /// Initializes a new instance of the ChatManager class.
    /// </summary>
    public ChatManager(IChatStyleManager chatStyleManager,
        ICommandManager commandManager,
        IChatEmotionsManager chatEmotionsManager,
        IChatlogManager chatlogManager,
        IWordFilterManager wordFilterManager,
        IPetCommandManager petCommandManager,
        IPetLocale petLocale,
        ILogger<ChatManager> logger)
    {
        _emotions = chatEmotionsManager;
        _logs = chatlogManager;
        _filter = wordFilterManager;
        _commands = commandManager;
        _petCommands = petCommandManager;
        _petLocale = petLocale;
        _chatStyles = chatStyleManager;
        _logger = logger;
    }

    public void Init()
    {
        _chatStyles.Init();
        _filter.Init();
        _petCommands.Init();
        _petLocale.Init();
        _logger.LogInformation("Chat Manager -> LOADED");
    }

    public IChatEmotionsManager GetEmotions() => _emotions;

    public IChatlogManager GetLogs() => _logs;

    public IWordFilterManager GetFilter() => _filter;

    public ICommandManager GetCommands() => _commands;

    public IPetCommandManager GetPetCommands() => _petCommands;

    public IPetLocale GetPetLocale() => _petLocale;

    public IChatStyleManager GetChatStyles() => _chatStyles;
}