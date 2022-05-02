﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
       Task IgnoreUser(Habbo uid, Habbo? ignoreid);
       Task UnIgnoreUser(Habbo uid, Habbo? ignoreid);

    }
}
