using Plus.Utilities.DependencyInjection;

namespace Plus.Core
{
    [Transient]
    public interface IStartable
    {
        Task Start();
    }
}
