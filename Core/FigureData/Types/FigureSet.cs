namespace Plus.Core.FigureData.Types;

internal class FigureSet
{
    public FigureSet(SetType type, int palletId)
    {
        Type = type;
        PalletId = palletId;
        Sets = new Dictionary<int, Set>();
    }

    public SetType Type { get; set; }
    public int PalletId { get; set; }
    public Dictionary<int, Set> Sets { get; set; }
}