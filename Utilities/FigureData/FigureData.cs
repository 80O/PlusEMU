using Plus.Utilities.FigureData.Types;
using System.Text.Json.Serialization;

namespace Plus.Utilities.FigureData;

public class FigureData
{
    [JsonPropertyName("setTypes")]
    public List<SetType>? SetTypes { get; set; }

    [JsonPropertyName("palettes")]
    public List<Palette>? Palettes { get; set; }

}
