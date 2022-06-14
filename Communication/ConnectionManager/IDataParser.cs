namespace Plus.Communication.ConnectionManager;

public interface IDataParser : IDisposable, ICloneable
{
    void HandlePacketData(byte[] packet);
}