namespace Plus.HabboHotel.Catalog.Clothing;

public interface IClothingManager
{
    ICollection<ClothingItem> GetClothingAllParts { get; }
    void Init();
    bool TryGetClothing(int itemId, out ClothingItem clothing);
}