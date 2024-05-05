using System.Text.Json.Serialization;

namespace Plus.Utilities.FigureData.Types;
public class Part
{ 
    [JsonPropertyName("colorindex")]
    public int ColorIndex { get; set; }
}
