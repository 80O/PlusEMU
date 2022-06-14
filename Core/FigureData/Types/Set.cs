namespace Plus.Core.FigureData.Types;

internal class Set
{
    public Set(int id, string gender, int clubLevel, bool colorable, bool selectable, bool preselectable)
    {
        Id = id;
        Gender = gender;
        ClubLevel = clubLevel;
        Colorable = colorable;
        Selectable = selectable;
        Preselectable = preselectable;
        Parts = new Dictionary<string, Part>();
    }

    public int Id { get; set; }
    public string Gender { get; set; }
    public int ClubLevel { get; set; }
    public bool Colorable { get; set; }
    public bool Selectable { get; set; }
    public bool Preselectable { get; set; }
    public Dictionary<string, Part> Parts { get; set; }
}