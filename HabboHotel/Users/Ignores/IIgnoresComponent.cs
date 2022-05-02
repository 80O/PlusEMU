using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Ignores
{
    public interface IIgnoresComponent
    {
       bool Init(Habbo player);
       bool TryGet(Habbo userId);
       bool TryAdd(Habbo userId);
       bool TryRemove(Habbo userId);
       ICollection<int> IgnoredUserIds();
       void Dispose();
       IReadOnlyCollection<string> GetIgnoredUsers(Habbo uid);
       Task IgnoreUser(Habbo uid, Habbo? ignoredid);
       Task UnIgnoreUser(Habbo uid, Habbo? ignoredid);

    }
}
