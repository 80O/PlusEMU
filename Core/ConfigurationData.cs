namespace Plus.Core;

public class ConfigurationData
{
    public Dictionary<string, string> Data;

    public ConfigurationData(string filePath, bool maynotexist = false)
    {
        Data = new Dictionary<string, string>();
        if (!File.Exists(filePath))
        {
            if (!maynotexist)
                throw new ArgumentException("Unable to locate configuration file at '" + filePath + "'.");
            return;
        }
        try
        {
            using var stream = new StreamReader(filePath);
            string line;
            while ((line = stream.ReadLine()) != null)
            {
                if (line.Length < 1 || line.StartsWith("#")) continue;
                var delimiterIndex = line.IndexOf('=');
                if (delimiterIndex != -1)
                {
                    var key = line.Substring(0, delimiterIndex);
                    var val = line.Substring(delimiterIndex + 1);
                    Data.Add(key, val);
                }
            }
        }
        catch (Exception e)
        {
            throw new ArgumentException("Could not process configuration file: " + e.Message);
        }
    }
}