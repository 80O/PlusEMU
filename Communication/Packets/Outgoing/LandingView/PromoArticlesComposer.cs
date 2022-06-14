using Plus.HabboHotel.LandingView.Promotions;

namespace Plus.Communication.Packets.Outgoing.LandingView;

internal class PromoArticlesComposer : ServerPacket
{
    public PromoArticlesComposer(ICollection<Promotion> landingPromotions)
        : base(ServerPacketHeader.PromoArticlesMessageComposer)
    {
        WriteInteger(landingPromotions.Count); //Count
        foreach (var promotion in landingPromotions.ToList())
        {
            WriteInteger(promotion.Id); //ID
            WriteString(promotion.Title); //Title
            WriteString(promotion.Text); //Text
            WriteString(promotion.ButtonText); //Button text
            WriteInteger(promotion.ButtonType); //Link type 0 and 3
            WriteString(promotion.ButtonLink); //Link to article
            WriteString(promotion.ImageLink); //Image link
        }
    }
}