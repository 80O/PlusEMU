using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.Trading;

namespace Plus.Communication.Packets.Outgoing.Inventory.Trading;

internal class TradingUpdateComposer : ServerPacket
{
    public TradingUpdateComposer(Trade trade)
        : base(ServerPacketHeader.TradingUpdateMessageComposer)
    {
        foreach (var user in trade.Users.ToList())
        {
            WriteInteger(user.RoomUser.UserId);
            WriteInteger(user.OfferedItems.Count);
            foreach (var item in user.OfferedItems.Values)
            {
                WriteInteger(item.Id);
                WriteString(item.GetBaseItem().Type.ToString().ToLower());
                WriteInteger(item.Id);
                WriteInteger(item.Data.SpriteId);
                WriteInteger(0); //Not sure.
                if (item.LimitedNo > 0)
                {
                    WriteBoolean(false); //Stackable
                    WriteInteger(256);
                    WriteString("");
                    WriteInteger(item.LimitedNo);
                    WriteInteger(item.LimitedTot);
                }
                else
                {
                    WriteBoolean(true); //Stackable
                    WriteInteger(0);
                    WriteString("");
                }
                WriteInteger(0);
                WriteInteger(0);
                WriteInteger(0);
                if (item.GetBaseItem().Type == 's')
                    WriteInteger(0);
            }
            WriteInteger(user.OfferedItems.Count); //Item Count
            WriteInteger(user.OfferedItems.Values.Where(x => x.Data.InteractionType == InteractionType.Exchange).Sum(t => t.Data.BehaviourData));
        }
    }
}