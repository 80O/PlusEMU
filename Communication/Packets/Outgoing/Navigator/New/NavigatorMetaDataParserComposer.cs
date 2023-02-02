using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Outgoing.Navigator.New;

// TODO @80O: Implement
public class NavigatorMetaDataParserComposer : IServerPacket
{
    private readonly IReadOnlyCollection<TopLevelItem> _topLevelItems;
    public uint MessageId => ServerPacketHeader.NavigatorMetaDataParserComposer;

    public NavigatorMetaDataParserComposer(IReadOnlyCollection<TopLevelItem> topLevelItems)
    {
        _topLevelItems = topLevelItems;
    }

    public void Compose(IOutgoingPacket packet)
    {
        packet.WriteInteger(_topLevelItems.Count); //Count
        foreach (var topLevelItem in _topLevelItems.ToList())
        {
            //TopLevelContext
            packet.WriteString(topLevelItem.SearchCode); //Search code
            packet.WriteInteger(0); //Count of saved searches?
            /*{
                //SavedSearch
                base.WriteInteger(TopLevelItem.Id);//Id
               base.WriteString(TopLevelItem.SearchCode);//Search code
               base.WriteString(TopLevelItem.Filter);//Filter
               base.WriteString(TopLevelItem.Localization);//localization
            }*/
        }
    }
}