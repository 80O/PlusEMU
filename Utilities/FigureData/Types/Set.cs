using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Plus.Utilities.FigureData.Types;
public class Set
{
    [JsonIgnore]
    public string SetTypeReference { get; set; }

    [JsonIgnore]
    public string Hash { get; set; }

    [JsonIgnore]
    public int Colors { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    [JsonPropertyName("club")]
    public int Club { get; set; }

    [JsonPropertyName("colorable")]
    public bool Colorable { get; set; }

    [JsonPropertyName("selectable")]
    public bool Selectable { get; set; }

    [JsonPropertyName("preselectable")]
    public bool Preselectable { get; set; }

    [JsonPropertyName("sellable")]
    public bool Sellable { get; set; }

    [JsonPropertyName("parts")]
    public List<Part> Parts { get; set; }
}