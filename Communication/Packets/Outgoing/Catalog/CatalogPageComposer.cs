using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Catalog;

public class CatalogPageComposer : ServerPacket
{
    public CatalogPageComposer(CatalogPage page, string cataMode)
        : base(ServerPacketHeader.CatalogPageMessageComposer)
    {
        WriteInteger(page.Id);
        WriteString(cataMode);
        WriteString(page.Template);
        WriteInteger(page.PageStrings1.Count);
        foreach (var s in page.PageStrings1) WriteString(s);
        WriteInteger(page.PageStrings2.Count);
        foreach (var s in page.PageStrings2) WriteString(s);
        if (!page.Template.Equals("frontpage") && !page.Template.Equals("club_buy"))
        {
            WriteInteger(page.Items.Count);
            foreach (var item in page.Items.Values)
            {
                WriteInteger(item.Id);
                WriteString(item.Name);
                WriteBoolean(false); //IsRentable
                WriteInteger(item.CostCredits);
                if (item.CostDiamonds > 0)
                {
                    WriteInteger(item.CostDiamonds);
                    WriteInteger(5); // Diamonds
                }
                else
                {
                    WriteInteger(item.CostPixels);
                    WriteInteger(0); // Type of PixelCost
                }
                WriteBoolean(ItemUtility.CanGiftItem(item));
                if (item.Data.InteractionType == InteractionType.Deal || item.Data.InteractionType == InteractionType.Roomdeal)
                {
                    CatalogDeal deal = null;
                    if (!PlusEnvironment.GetGame().GetCatalog().TryGetDeal(item.Data.BehaviourData, out deal))
                        WriteInteger(0); //Count
                    else
                    {
                        WriteInteger(deal.ItemDataList.Count);
                        foreach (var dealItem in deal.ItemDataList.ToList())
                        {
                            WriteString(dealItem.Data.Type.ToString());
                            WriteInteger(dealItem.Data.SpriteId);
                            WriteString("");
                            WriteInteger(dealItem.Amount);
                            WriteBoolean(false);
                        }
                    }
                }
                else
                {
                    WriteInteger(string.IsNullOrEmpty(item.Badge) ? 1 : 2); //Count 1 item if there is no badge, otherwise count as 2.
                    if (!string.IsNullOrEmpty(item.Badge))
                    {
                        WriteString("b");
                        WriteString(item.Badge);
                    }
                    WriteString(item.Data.Type.ToString());
                    if (item.Data.Type.ToString().ToLower() == "b")
                    {
                        //This is just a badge, append the name.
                        WriteString(item.Data.ItemName);
                    }
                    else
                    {
                        WriteInteger(item.Data.SpriteId);
                        if (item.Data.InteractionType == InteractionType.Wallpaper || item.Data.InteractionType == InteractionType.Floor || item.Data.InteractionType == InteractionType.Landscape)
                            WriteString(item.Name.Split('_')[2]);
                        else if (item.Data.InteractionType == InteractionType.Bot) //Bots
                        {
                            CatalogBot catalogBot = null;
                            if (!PlusEnvironment.GetGame().GetCatalog().TryGetBot(item.ItemId, out catalogBot))
                                WriteString("hd-180-7.ea-1406-62.ch-210-1321.hr-831-49.ca-1813-62.sh-295-1321.lg-285-92");
                            else
                                WriteString(catalogBot.Figure);
                        }
                        else if (item.ExtraData != null) WriteString(item.ExtraData != null ? item.ExtraData : string.Empty);
                        WriteInteger(item.Amount);
                        WriteBoolean(item.IsLimited); // IsLimited
                        if (item.IsLimited)
                        {
                            WriteInteger(item.LimitedEditionStack);
                            WriteInteger(item.LimitedEditionStack - item.LimitedEditionSells);
                        }
                    }
                }
                WriteInteger(0); //club_level
                WriteBoolean(ItemUtility.CanSelectAmount(item));
                WriteBoolean(false); // TODO: Figure out
                WriteString(""); //previewImage -> e.g; catalogue/pet_lion.png
            }
        }
        else
            WriteInteger(0);
        WriteInteger(-1);
        WriteBoolean(false);
        WriteInteger(PlusEnvironment.GetGame().GetCatalog().GetPromotions().ToList().Count); //Count
        foreach (var promotion in PlusEnvironment.GetGame().GetCatalog().GetPromotions().ToList())
        {
            WriteInteger(promotion.Id);
            WriteString(promotion.Title);
            WriteString(promotion.Image);
            WriteInteger(promotion.Unknown);
            WriteString(promotion.PageLink);
            WriteInteger(promotion.ParentId);
        }
    }
}