using System;
using System.Collections.Generic;
using System.Text;

namespace GamePassScores.Models
{
    public enum SubscriptionServices { XboxGamePass, EAPlay }
    public enum Platform {Unknown, Xbox, Xbox360, XboxOne, XboxOneX, XboxSeriesX, XboxSeriesS, PC, PS1, PS2, PS3, PS4, PS5, Switch };
    public enum InVaultTime { RecentlyAdded, LeavingSoon, Normal};
}
