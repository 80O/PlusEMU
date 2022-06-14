using Plus.Utilities;
using System.Data;

namespace Plus.Communication.Packets.Outgoing.Moderation;

internal class ModeratorUserInfoComposer : ServerPacket
{
    public ModeratorUserInfoComposer(DataRow user, DataRow info)
        : base(ServerPacketHeader.ModeratorUserInfoMessageComposer)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(info["trading_locked"]));
        WriteInteger(user != null ? Convert.ToInt32(user["id"]) : 0);
        WriteString(user != null ? Convert.ToString(user["username"]) : "Unknown");
        WriteString(user != null ? Convert.ToString(user["look"]) : "Unknown");
        WriteInteger(user != null ? Convert.ToInt32(Math.Ceiling((UnixTimestamp.GetNow() - Convert.ToDouble(user["account_created"])) / 60)) : 0);
        WriteInteger(user != null ? Convert.ToInt32(Math.Ceiling((UnixTimestamp.GetNow() - Convert.ToDouble(user["last_online"])) / 60)) : 0);
        WriteBoolean(user != null ? PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(Convert.ToInt32(user["id"])) != null : false);
        WriteInteger(info != null ? Convert.ToInt32(info["cfhs"]) : 0);
        WriteInteger(info != null ? Convert.ToInt32(info["cfhs_abusive"]) : 0);
        WriteInteger(info != null ? Convert.ToInt32(info["cautions"]) : 0);
        WriteInteger(info != null ? Convert.ToInt32(info["bans"]) : 0);
        WriteInteger(info != null ? Convert.ToInt32(info["trading_locks_count"]) : 0); //Trading lock counts
        WriteString(Convert.ToDouble(info["trading_locked"]) != 0 ? origin.ToString("dd/MM/yyyy HH:mm:ss") : "0"); //Trading lock
        WriteString(""); //Purchases
        WriteInteger(0); //Itendity information tool
        WriteInteger(0); //Id bans.
        WriteString(user != null ? Convert.ToString(user["mail"]) : "Unknown");
        WriteString(""); //user_classification
    }
}