﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Plus.HabboHotel.LandingView.Promotions;

namespace Plus.HabboHotel.LandingView;

public interface ILandingViewManager
{
    Task Reload();
    ICollection<Promotion> GetPromotionItems();
}