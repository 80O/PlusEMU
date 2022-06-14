using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Catalog;

public class CatalogIndexComposer : ServerPacket
{
    public CatalogIndexComposer(GameClient sesion, ICollection<CatalogPage> pages)
        : base(ServerPacketHeader.CatalogIndexMessageComposer)
    {
        WriteRootIndex(sesion, pages);
        foreach (var parent in pages)
        {
            if (parent.ParentId != -1 || parent.MinimumRank > sesion.GetHabbo().Rank || parent.MinimumVip > sesion.GetHabbo().VipRank && sesion.GetHabbo().Rank == 1)
                continue;
            WritePage(parent, CalcTreeSize(sesion, pages, parent.Id));
            foreach (var child in pages)
            {
                if (child.ParentId != parent.Id || child.MinimumRank > sesion.GetHabbo().Rank || child.MinimumVip > sesion.GetHabbo().VipRank && sesion.GetHabbo().Rank == 1)
                    continue;
                if (child.Enabled)
                    WritePage(child, CalcTreeSize(sesion, pages, child.Id));
                else
                    WriteNodeIndex(child, CalcTreeSize(sesion, pages, child.Id));
                foreach (var subChild in pages)
                {
                    if (subChild.ParentId != child.Id || subChild.MinimumRank > sesion.GetHabbo().Rank)
                        continue;
                    WritePage(subChild, 0);
                }
            }
        }
        WriteBoolean(false);
        WriteString("NORMAL");
    }

    public void WriteRootIndex(GameClient session, ICollection<CatalogPage> pages)
    {
        WriteBoolean(true);
        WriteInteger(0);
        WriteInteger(-1);
        WriteString("root");
        WriteString(string.Empty);
        WriteInteger(0);
        WriteInteger(CalcTreeSize(session, pages, -1));
    }

    public void WriteNodeIndex(CatalogPage page, int treeSize)
    {
        WriteBoolean(page.Visible);
        WriteInteger(page.Icon);
        WriteInteger(-1);
        WriteString(page.PageLink);
        WriteString(page.Caption);
        WriteInteger(0);
        WriteInteger(treeSize);
    }

    public void WritePage(CatalogPage page, int treeSize)
    {
        WriteBoolean(page.Visible);
        WriteInteger(page.Icon);
        WriteInteger(page.Id);
        WriteString(page.PageLink);
        WriteString(page.Caption);
        WriteInteger(page.ItemOffers.Count);
        foreach (var i in page.ItemOffers.Keys) WriteInteger(i);
        WriteInteger(treeSize);
    }

    public int CalcTreeSize(GameClient session, ICollection<CatalogPage> pages, int parentId)
    {
        var i = 0;
        foreach (var page in pages)
        {
            if (page.MinimumRank > session.GetHabbo().Rank || page.MinimumVip > session.GetHabbo().VipRank && session.GetHabbo().Rank == 1 || page.ParentId != parentId)
                continue;
            if (page.ParentId == parentId)
                i++;
        }
        return i;
    }
}