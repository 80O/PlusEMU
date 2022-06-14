using Plus.Utilities.DependencyInjection;

namespace Plus.Communication.Rcon.Commands;

[Transient]
public interface IRconCommand
{
    string Key { get; }
    string Parameters { get; }
    string Description { get; }
    Task<bool> TryExecute(string[] parameters);
}