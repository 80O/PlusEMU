using Plus.HabboHotel.GameClients;
using Plus.Database;

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
        using var dbClient = _database.GetQueryReactor();
        dbClient.SetQuery("UPDATE users SET volume = @volume WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
        dbClient.AddParameter("volume", volume);
        dbClient.RunQuery();
    }
}