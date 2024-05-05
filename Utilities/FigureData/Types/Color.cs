using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Plus.Utilities.FigureData.Types;
public class Color
{
    [JsonIgnore]
    public int PaletteId { get; set; }  // Add this property
    
    [JsonIgnore]
    public int primaryId { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("club")]
    public int Club { get; set; }

    [JsonPropertyName("selectable")]
    public bool Selectable { get; set; }

    [JsonPropertyName("hexCode")]
    public string HexCode { get; set; }
}