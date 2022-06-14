namespace Plus.Database.Interfaces;

public interface IQueryAdapter : IRegularQueryAdapter, IDisposable
{
    long InsertQuery();
    void RunQuery();
}