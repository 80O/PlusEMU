using System.Xml;
using Microsoft.Extensions.Logging;
using Plus.Core.FigureData.Types;
using Plus.HabboHotel.Users.Clothing.Parts;
using Plus.Utilities;

namespace Plus.Core.FigureData;

public class FigureDataManager : IFigureDataManager
{
    private readonly ILogger<FigureDataManager> _logger;
    private readonly Dictionary<int, Palette> _palettes; //pallet id, Pallet

    private readonly List<string> _requirements;
    private readonly Dictionary<string, FigureSet> _setTypes; //type (hr, ch, etc), Set

    public FigureDataManager(ILogger<FigureDataManager> logger)
    {
        _logger = logger;
        _palettes = new Dictionary<int, Palette>();
        _setTypes = new Dictionary<string, FigureSet>();
        _requirements = new List<string>
        {
            "hd",
            "ch",
            "lg"
        };
    }

    public void Init()
    {
        if (_palettes.Count > 0)
            _palettes.Clear();
        if (_setTypes.Count > 0)
            _setTypes.Clear();
        var projectSolutionPath = Directory.GetCurrentDirectory();
        var xDoc = new XmlDocument();
        xDoc.Load(projectSolutionPath + "//Config//figuredata.xml");
        var colors = xDoc.GetElementsByTagName("colors");
        foreach (XmlNode node in colors)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                _palettes.Add(Convert.ToInt32(child.Attributes["id"].Value), new Palette(Convert.ToInt32(child.Attributes["id"].Value)));
                foreach (XmlNode sub in child.ChildNodes)
                {
                    _palettes[Convert.ToInt32(child.Attributes["id"].Value)].Colors.Add(Convert.ToInt32(sub.Attributes["id"].Value),
                        new Color(Convert.ToInt32(sub.Attributes["id"].Value), Convert.ToInt32(sub.Attributes["index"].Value), Convert.ToInt32(sub.Attributes["club"].Value),
                            Convert.ToInt32(sub.Attributes["selectable"].Value) == 1, Convert.ToString(sub.InnerText)));
                }
            }
        }
        var sets = xDoc.GetElementsByTagName("sets");
        foreach (XmlNode node in sets)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                _setTypes.Add(child.Attributes["type"].Value, new FigureSet(SetTypeUtility.GetSetType(child.Attributes["type"].Value), Convert.ToInt32(child.Attributes["paletteid"].Value)));
                foreach (XmlNode sub in child.ChildNodes)
                {
                    _setTypes[child.Attributes["type"].Value].Sets.Add(Convert.ToInt32(sub.Attributes["id"].Value),
                        new Set(Convert.ToInt32(sub.Attributes["id"].Value), Convert.ToString(sub.Attributes["gender"].Value), Convert.ToInt32(sub.Attributes["club"].Value),
                            Convert.ToInt32(sub.Attributes["colorable"].Value) == 1, Convert.ToInt32(sub.Attributes["selectable"].Value) == 1,
                            Convert.ToInt32(sub.Attributes["preselectable"].Value) == 1));
                    foreach (XmlNode subb in sub.ChildNodes)
                    {
                        if (subb.Attributes["type"] != null)
                        {
                            _setTypes[child.Attributes["type"].Value].Sets[Convert.ToInt32(sub.Attributes["id"].Value)].Parts.Add(
                                Convert.ToInt32(subb.Attributes["id"].Value) + "-" + subb.Attributes["type"].Value,
                                new Part(Convert.ToInt32(subb.Attributes["id"].Value), SetTypeUtility.GetSetType(child.Attributes["type"].Value),
                                    Convert.ToInt32(subb.Attributes["colorable"].Value) == 1, Convert.ToInt32(subb.Attributes["index"].Value), Convert.ToInt32(subb.Attributes["colorindex"].Value)));
                        }
                    }
                }
            }
        }

        //Faceless.
        _setTypes["hd"].Sets.Add(99999, new Set(99999, "U", 0, true, false, false));
        _logger.LogInformation("Loaded " + _palettes.Count + " Color Palettes");
        _logger.LogInformation("Loaded " + _setTypes.Count + " Set Types");
    }

    public string ProcessFigure(string figure, string gender, ICollection<ClothingParts> clothingParts, bool hasHabboClub)
    {
        figure = figure.ToLower();
        gender = gender.ToUpper();
        var rebuildFigure = string.Empty;
        var figureParts = figure.Split('.');
        foreach (var part in figureParts.ToList())
        {
            var type = part.Split('-')[0];
            if (_setTypes.TryGetValue(type, out var figureSet))
            {
                var partId = Convert.ToInt32(part.Split('-')[1]);
                var colorId = 0;
                var secondColorId = 0;
                if (figureSet.Sets.TryGetValue(partId, out var set))
                {
                    if (set.Gender != gender && set.Gender != "U")
                    {
                        if (figureSet.Sets.Count(x => x.Value.Gender == gender || x.Value.Gender == "U") > 0)
                        {
                            partId = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value.Id;

                            //Fetch the new set.
                            figureSet.Sets.TryGetValue(partId, out set);
                            colorId = GetRandomColor(figureSet.PalletId);
                        }
                    }
                    if (set.Colorable)
                    {
                        //Couldn't think of a better way to split the colors, if I looped the parts I still have to remove Type-PartId, then loop color 1 & color 2. Meh
                        var splitterCounter = part.Count(x => x == '-');
                        if (splitterCounter == 2 || splitterCounter == 3)
                        {
                            if (!string.IsNullOrEmpty(part.Split('-')[2]))
                            {
                                if (int.TryParse(part.Split('-')[2], out colorId))
                                {
                                    colorId = Convert.ToInt32(part.Split('-')[2]);
                                    var palette = GetPalette(colorId);
                                    if (palette != null && colorId != 0)
                                    {
                                        if (figureSet.PalletId != palette.Id) colorId = GetRandomColor(figureSet.PalletId);
                                    }
                                    else if (palette == null && colorId != 0) colorId = GetRandomColor(figureSet.PalletId);
                                }
                                else
                                    colorId = 0;
                            }
                            else
                                colorId = 0;
                        }
                        if (splitterCounter == 3)
                        {
                            if (!string.IsNullOrEmpty(part.Split('-')[3]))
                            {
                                if (int.TryParse(part.Split('-')[3], out secondColorId))
                                {
                                    secondColorId = Convert.ToInt32(part.Split('-')[3]);
                                    var palette = GetPalette(secondColorId);
                                    if (palette != null && secondColorId != 0)
                                    {
                                        if (figureSet.PalletId != palette.Id) secondColorId = GetRandomColor(figureSet.PalletId);
                                    }
                                    else if (palette == null && secondColorId != 0) secondColorId = GetRandomColor(figureSet.PalletId);
                                }
                                else
                                    secondColorId = 0;
                            }
                            else
                                secondColorId = 0;
                        }
                    }
                    else
                    {
                        var ignore = new[] { "ca", "wa" };
                        if (ignore.Contains(type))
                            if (!string.IsNullOrEmpty(part.Split('-')[2]))
                                colorId = Convert.ToInt32(part.Split('-')[2]);
                    }
                    if (set.ClubLevel > 0 && !hasHabboClub)
                    {
                        partId = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U" && x.Value.ClubLevel == 0).Value.Id;
                        figureSet.Sets.TryGetValue(partId, out set);
                        colorId = GetRandomColor(figureSet.PalletId);
                    }
                    if (secondColorId == 0)
                        rebuildFigure = rebuildFigure + type + "-" + partId + "-" + colorId + ".";
                    else
                        rebuildFigure = rebuildFigure + type + "-" + partId + "-" + colorId + "-" + secondColorId + ".";
                }
            }
        }
        foreach (var requirement in _requirements)
        {
            if (!rebuildFigure.Contains(requirement))
            {
                if (requirement == "ch" && gender == "M")
                    continue;
                if (_setTypes.TryGetValue(requirement, out var figureSet))
                {
                    var set = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value;
                    if (set != null)
                    {
                        var partId = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value.Id;
                        var colorId = GetRandomColor(figureSet.PalletId);
                        rebuildFigure = rebuildFigure + requirement + "-" + partId + "-" + colorId + ".";
                    }
                }
            }
        }
        if (clothingParts != null)
        {
            var purchasableParts = PlusEnvironment.GetGame().GetCatalog().GetClothingManager().GetClothingAllParts;
            figureParts = rebuildFigure.TrimEnd('.').Split('.');
            foreach (var part in figureParts.ToList())
            {
                var partId = Convert.ToInt32(part.Split('-')[1]);
                if (purchasableParts.Count(x => x.PartIds.Contains(partId)) > 0)
                {
                    if (clothingParts.Count(x => x.PartId == partId) == 0)
                    {
                        var type = part.Split('-')[0];
                        if (_setTypes.TryGetValue(type, out var figureSet))
                        {
                            var set = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value;
                            if (set != null)
                            {
                                partId = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value.Id;
                                var colorId = GetRandomColor(figureSet.PalletId);
                                rebuildFigure = rebuildFigure + type + "-" + partId + "-" + colorId + ".";
                            }
                        }
                    }
                }
            }
        }
        return rebuildFigure;
    }

    public Palette GetPalette(int colorId)
    {
        return _palettes.FirstOrDefault(x => x.Value.Colors.ContainsKey(colorId)).Value;
    }

    public bool TryGetPalette(int palletId, out Palette palette) => _palettes.TryGetValue(palletId, out palette);

    public int GetRandomColor(int palletId) => _palettes[palletId].Colors.FirstOrDefault().Value.Id;

    public string FilterFigure(string figure)
    {
        return StringCharFilter.IsValid(figure) ? figure : IFigureDataManager.DefaultFigure;
    }
}