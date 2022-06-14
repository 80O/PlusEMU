using Plus.HabboHotel.Items.Wired;

namespace Plus.HabboHotel.Items;

public class ItemData
{
    public ItemData(int id, int sprite, string name, string publicName, string type, int width, int length, double height, bool stackable, bool walkable, bool isSeat,
        bool allowRecycle, bool allowTrade, bool allowMarketplaceSell, bool allowGift, bool allowInventoryStack, InteractionType interactionType, int behaviourData, int modes,
        string vendingIds, string adjustableHeights, int effectId, bool isRare, bool extraRot)
    {
        Id = id;
        SpriteId = sprite;
        ItemName = name;
        PublicName = publicName;
        Type = char.Parse(type);
        Width = width;
        Length = length;
        Height = height;
        Stackable = stackable;
        Walkable = walkable;
        IsSeat = isSeat;
        AllowEcotronRecycle = allowRecycle;
        AllowTrade = allowTrade;
        AllowMarketplaceSell = allowMarketplaceSell;
        AllowGift = allowGift;
        AllowInventoryStack = allowInventoryStack;
        InteractionType = interactionType;
        BehaviourData = behaviourData;
        Modes = modes;
        VendingIds = new List<int>();
        if (vendingIds.Contains(","))
        {
            foreach (var vendingId in vendingIds.Split(','))
            {
                try
                {
                    VendingIds.Add(int.Parse(vendingId));
                }
                catch
                {
                    Console.WriteLine("Error with Item " + ItemName + " - Vending Ids");
                }
            }
        }
        else if (!string.IsNullOrEmpty(vendingIds) && int.Parse(vendingIds) > 0)
            VendingIds.Add(int.Parse(vendingIds));
        AdjustableHeights = new List<double>();
        if (adjustableHeights.Contains(","))
            foreach (var h in adjustableHeights.Split(','))
                AdjustableHeights.Add(double.Parse(h));
        else if (!string.IsNullOrEmpty(adjustableHeights) && double.Parse(adjustableHeights) > 0)
            AdjustableHeights.Add(double.Parse(adjustableHeights));
        EffectId = effectId;
        var wiredId = 0;
        if (InteractionType == InteractionType.WiredCondition || InteractionType == InteractionType.WiredTrigger || InteractionType == InteractionType.WiredEffect)
            wiredId = BehaviourData;
        WiredType = WiredBoxTypeUtility.FromWiredId(wiredId);
        IsRare = isRare;
        ExtraRot = extraRot;
    }

    public int Id { get; set; }
    public int SpriteId { get; set; }
    public string ItemName { get; set; }
    public string PublicName { get; set; }
    public char Type { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
    public double Height { get; set; }
    public bool Stackable { get; set; }
    public bool Walkable { get; set; }
    public bool IsSeat { get; set; }
    public bool AllowEcotronRecycle { get; set; }
    public bool AllowTrade { get; set; }
    public bool AllowMarketplaceSell { get; set; }
    public bool AllowGift { get; set; }
    public bool AllowInventoryStack { get; set; }
    public InteractionType InteractionType { get; set; }
    public int BehaviourData { get; set; }
    public int Modes { get; set; }
    public List<int> VendingIds { get; set; }
    public List<double> AdjustableHeights { get; set; }
    public int EffectId { get; set; }
    public WiredBoxType WiredType { get; set; }
    public bool IsRare { get; set; }
    public bool ExtraRot { get; set; }
}