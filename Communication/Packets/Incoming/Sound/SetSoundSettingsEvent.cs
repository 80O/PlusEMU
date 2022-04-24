using Plus.Database;
using Plus.HabboHotel.GameClients;
using Dapper;

namespace Plus.Communication.Packets.Incoming.Sound;

internal class SetSoundSettingsEvent : IPacketEvent
{
    private readonly IDatabase _database;

    public SetSoundSettingsEvent(IDatabase database)
    {
        _database = database;
    }

    public void Parse(GameClient session, ClientPacket packet)
    {
        var volume = "";
        for (var i = 0; i < 3; i++)
        {
            var vol = packet.PopInt();
            if (vol < 0 || vol > 100) vol = 100;
            if (i < 2)
                volume += vol + ",";
            else
                volume += vol;
        }
        using var connection = _database.Connection();
        connection.Execute("UPDATE users SET volume = @volume WHERE id = @id LIMIT 1", new { volume = volume, id = session.GetHabbo().Id });
    }
}