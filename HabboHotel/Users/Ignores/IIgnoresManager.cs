using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Ignores
{
    public  interface IIgnoresManager
    {
        Task UnIgnoreUser(int uid, int ignoreid);
    }
}
