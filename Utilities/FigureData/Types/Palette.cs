using System.Text.Json.Serialization;

namespace Plus.Utilities.FigureData.Types;
public class Palette
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("colors")]
    public List<Color> Colors { get; set; }
}