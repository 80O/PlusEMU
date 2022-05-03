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
       Task<IReadOnlyCollection<string>> GetIgnoredUsers(Habbo uid);
       Task<Habbo?> IgnoreUser(Habbo uid, Habbo? ignoredid);
       Task<Habbo?> UnIgnoreUser(Habbo uid, Habbo? ignoredid);

    }
}
