using System.Data;
using System.Text;

namespace Plus.HabboHotel.Items.Data.Moodlight;

public class MoodlightData
{
    public int CurrentPreset;
    public bool Enabled;
    public int ItemId;

    public List<MoodlightPreset> Presets;

    public MoodlightData(int itemId)
    {
        ItemId = itemId;
        DataRow row = null;
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("SELECT enabled,current_preset,preset_one,preset_two,preset_three FROM room_items_moodlight WHERE item_id = '" + itemId + "' LIMIT 1");
            row = dbClient.GetRow();
        }
        if (row == null)
        {
            using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.RunQuery("INSERT INTO `room_items_moodlight` (item_id,enabled,current_preset,preset_one,preset_two,preset_three) VALUES (" + itemId +
                              ",0,1,'#000000,255,0','#000000,255,0','#000000,255,0')");
            dbClient.SetQuery("SELECT enabled,current_preset,preset_one,preset_two,preset_three FROM room_items_moodlight WHERE item_id=" + itemId + " LIMIT 1");
            row = dbClient.GetRow();
        }
        Enabled = PlusEnvironment.EnumToBool(row["enabled"].ToString());
        CurrentPreset = Convert.ToInt32(row["current_preset"]);
        Presets = new List<MoodlightPreset>();
        Presets.Add(GeneratePreset(Convert.ToString(row["preset_one"])));
        Presets.Add(GeneratePreset(Convert.ToString(row["preset_two"])));
        Presets.Add(GeneratePreset(Convert.ToString(row["preset_three"])));
    }

    public void Enable()
    {
        Enabled = true;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.RunQuery("UPDATE room_items_moodlight SET enabled = 1 WHERE item_id = '" + ItemId + "' LIMIT 1");
    }

    public void Disable()
    {
        Enabled = false;
        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
        dbClient.RunQuery("UPDATE room_items_moodlight SET enabled = 0 WHERE item_id = '" + ItemId + "' LIMIT 1");
    }

    public void UpdatePreset(int preset, string color, int intensity, bool bgOnly, bool hax = false)
    {
        if (!IsValidColor(color) || !IsValidIntensity(intensity) && !hax) return;
        string pr;
        switch (preset)
        {
            case 3:
                pr = "three";
                break;
            case 2:
                pr = "two";
                break;
            case 1:
            default:
                pr = "one";
                break;
        }
        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            dbClient.SetQuery("UPDATE room_items_moodlight SET preset_" + pr + " = '@color," + intensity + "," + PlusEnvironment.BoolToEnum(bgOnly) + "' WHERE item_id = '" + ItemId + "' LIMIT 1");
            dbClient.AddParameter("color", color);
            dbClient.RunQuery();
        }
        GetPreset(preset).ColorCode = color;
        GetPreset(preset).ColorIntensity = intensity;
        GetPreset(preset).BackgroundOnly = bgOnly;
    }

    public static MoodlightPreset GeneratePreset(string data)
    {
        var bits = data.Split(',');
        if (!IsValidColor(bits[0])) bits[0] = "#000000";
        return new MoodlightPreset(bits[0], int.Parse(bits[1]), PlusEnvironment.EnumToBool(bits[2]));
    }

    public MoodlightPreset GetPreset(int i)
    {
        i--;
        if (Presets[i] != null) return Presets[i];
        return new MoodlightPreset("#000000", 255, false);
    }

    public static bool IsValidColor(string colorCode)
    {
        switch (colorCode)
        {
            case "#000000":
            case "#0053F7":
            case "#EA4532":
            case "#82F349":
            case "#74F5F5":
            case "#E759DE":
            case "#F2F851":
                return true;
            default:
                return false;
        }
    }

    public static bool IsValidIntensity(int intensity)
    {
        if (intensity < 0 || intensity > 255) return false;
        return true;
    }

    public string GenerateExtraData()
    {
        var preset = GetPreset(CurrentPreset);
        var sb = new StringBuilder();
        sb.Append(Enabled ? 2 : 1);
        sb.Append(",");
        sb.Append(CurrentPreset);
        sb.Append(",");
        sb.Append(preset.BackgroundOnly ? 2 : 1);
        sb.Append(",");
        sb.Append(preset.ColorCode);
        sb.Append(",");
        sb.Append(preset.ColorIntensity);
        return sb.ToString();
    }
}