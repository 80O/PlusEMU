using Plus.Database.Interfaces;

namespace Plus.Database.Adapter;

public class NormalQueryReactor : QueryAdapter, IQueryAdapter
{
    public NormalQueryReactor(IDatabaseClient client)
        : base(client)
    {
        Command = client.CreateNewCommand();
    }

    public void Dispose()
    {
        Command.Dispose();
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}