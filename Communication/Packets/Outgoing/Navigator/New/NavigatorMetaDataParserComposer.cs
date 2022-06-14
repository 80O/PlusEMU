using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Outgoing.Navigator.New;

internal class NavigatorMetaDataParserComposer : ServerPacket
{
    public NavigatorMetaDataParserComposer(ICollection<TopLevelItem> topLevelItems)
        : base(ServerPacketHeader.NavigatorMetaDataParserMessageComposer)
    {
        //TODO: HMU here too, if you want saved searches to be fixed
        WriteInteger(topLevelItems.Count); //Count
        foreach (var topLevelItem in topLevelItems.ToList())
        {
            //TopLevelContext
            WriteString(topLevelItem.SearchCode); //Search code
            WriteInteger(0); //Count of saved searches?
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