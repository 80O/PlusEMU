namespace Plus.HabboHotel.Catalog;

public class CatalogPage
{
    public CatalogPage(int id, int parentId, string enabled, string caption, string pageLink, int icon, int minRank, int minVip,
        string visible, string template, string pageStrings1, string pageStrings2, Dictionary<int, CatalogItem> items, ref Dictionary<int, int> flatOffers)
    {
        Id = id;
        ParentId = parentId;
        Enabled = enabled.ToLower() == "1" ? true : false;
        Caption = caption;
        PageLink = pageLink;
        Icon = icon;
        MinimumRank = minRank;
        MinimumVip = minVip;
        Visible = visible.ToLower() == "1" ? true : false;
        Template = template;
        foreach (var str in pageStrings1.Split('|'))
        {
            if (PageStrings1 == null) PageStrings1 = new List<string>();
            PageStrings1.Add(str);
        }
        foreach (var str in pageStrings2.Split('|'))
        {
            if (PageStrings2 == null) PageStrings2 = new List<string>();
            PageStrings2.Add(str);
        }
        Items = items;
        ItemOffers = new Dictionary<int, CatalogItem>();
        foreach (var i in flatOffers.Keys)
        {
            if (flatOffers[i] == id)
            {
                foreach (var item in Items.Values)
                {
                    if (item.OfferId == i)
                    {
                        if (!ItemOffers.ContainsKey(i))
                            ItemOffers.Add(i, item);
                    }
                }
            }
        }
    }

    public int Id { get; set; }

    public int ParentId { get; set; }

    public bool Enabled { get; set; }

    public string Caption { get; set; }

    public string PageLink { get; set; }

    public int Icon { get; set; }

    public int MinimumRank { get; set; }

    public int MinimumVip { get; set; }

    public bool Visible { get; set; }

    public string Template { get; set; }

    public List<string> PageStrings1 { get; }

    public List<string> PageStrings2 { get; }

    public Dictionary<int, CatalogItem> Items { get; }

    public Dictionary<int, CatalogItem> ItemOffers { get; }

    public CatalogItem GetItem(int pId)
    {
        if (Items.ContainsKey(pId))
            return Items[pId];
        return null;
    }
}