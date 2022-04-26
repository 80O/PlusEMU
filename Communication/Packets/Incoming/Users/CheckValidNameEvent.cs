using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Filter;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Users;

internal class CheckValidNameEvent : IPacketEvent
{
    private readonly IWordFilterManager _wordFilterManager;
    private readonly IDatabase _database;

    public CheckValidNameEvent(IWordFilterManager wordFilterManager, IDatabase database)
    {
        _wordFilterManager = wordFilterManager;
        _database = database;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        bool inUse;
        var name = packet.PopString();
        using (var connection = _database.Connection())
        {
            var checkUser = connection.ExecuteScalar<int>("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1", new { name = name });
            inUse = checkUser == 1;
        }
        var letters = name.ToLower().ToCharArray();
        const string allowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";
        if (letters.Any(chr => !allowedCharacters.Contains(chr)))
        {
            session.SendPacket(new NameChangeUpdateComposer(name, 4));
            return Task.CompletedTask;
        }
        if (_wordFilterManager.IsFiltered(name))
        {
            session.SendPacket(new NameChangeUpdateComposer(name, 4));
            return Task.CompletedTask;
        }
        if (!session.GetHabbo().GetPermissions().HasRight("mod_tool") && name.ToLower().Contains("mod") || name.ToLower().Contains("adm") || name.ToLower().Contains("admin") ||
            name.ToLower().Contains("m0d"))
        {
            session.SendPacket(new NameChangeUpdateComposer(name, 4));
            return Task.CompletedTask;
        }
        if (!name.ToLower().Contains("mod") && (session.GetHabbo().Rank == 2 || session.GetHabbo().Rank == 3))
        {
            session.SendPacket(new NameChangeUpdateComposer(name, 4));
            return Task.CompletedTask;
        }
        if (name.Length > 15)
        {
            session.SendPacket(new NameChangeUpdateComposer(name, 3));
            return Task.CompletedTask;
        }
        if (name.Length < 3)
        {
            session.SendPacket(new NameChangeUpdateComposer(name, 2));
            return Task.CompletedTask;
        }
        if (inUse)
        {
            ICollection<string> suggestions = new List<string>();
            for (var i = 100; i < 103; i++) suggestions.Add(i.ToString());
            session.SendPacket(new NameChangeUpdateComposer(name, 5, suggestions));
            return Task.CompletedTask;
        }
        session.SendPacket(new NameChangeUpdateComposer(name, 0));
        return Task.CompletedTask;
    }
}