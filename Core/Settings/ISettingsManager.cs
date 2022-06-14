namespace Plus.Core.Settings;

public interface ISettingsManager
{
    string TryGetValue(string value);
    Task Reload();
}