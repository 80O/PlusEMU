using System.Text.Json.Serialization;

namespace Plus.Utilities.FigureData.Types;
public class SetType
{
    [JsonIgnore]
    public Dictionary<int, Set> IndexedSets { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("paletteId")]
    public int PaletteId { get; set; }

    [JsonPropertyName("mandatory_f_0")]
    public bool MandatoryF0 { get; set; }

    [JsonPropertyName("mandatory_f_1")]
    public bool MandatoryF1 { get; set; }

    [JsonPropertyName("mandatory_m_0")]
    public bool MandatoryM0 { get; set; }

    [JsonPropertyName("mandatory_m_1")]
    public bool MandatoryM1 { get; set; }

    [JsonPropertyName("sets")]
    public List<Set> Sets { get; set; }
}