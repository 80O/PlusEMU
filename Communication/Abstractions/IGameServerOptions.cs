namespace Plus.Communication.Abstractions
{
    public interface IGameServerOptions
    {
        string Name { get; }
        string Hostname { get; }
        int Port { get; }
    }
}