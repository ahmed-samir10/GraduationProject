using KonSanad.Repository.Entities;

using System;
using System.Linq;
using KonSanad.Repository.Entities;

namespace KonSanad.APIS.Helpers
{
    /// <summary>
    /// Encapsulates campaign presentation calculations.
    /// Mapping profile calls these methods; no calculation logic should live inside the profile.
    /// </summary>
    public static class CampaignMetrics
    {
        public static int CalculateProgress(Campaign s)
        {
            if (s == null) return 0;

            if (!s.GoalCash.HasValue || s.GoalCash.Value <= 0) return 0;

            var raised = s.DonationOrders != null
                ? s.DonationOrders.Sum(dono =>
                    dono.CashAmount ?? (dono.CashDonation != null ? dono.CashDonation.Amount : 0M))
                : 0M;

            var pct = (raised / s.GoalCash.Value) * 100M;

            if (pct > 100M) pct = 100M;
            if (pct < 0M) pct = 0M;

            return (int)pct;
        }

        public static string CalculateStatus(Campaign s)
        {
            if (s == null) return "نشطة";

            if (!s.GoalCash.HasValue || s.GoalCash.Value <= 0)
                return "نشطة";

            var raised = s.DonationOrders != null
                ? s.DonationOrders.Sum(dono =>
                    dono.CashAmount ?? (dono.CashDonation != null ? dono.CashDonation.Amount : 0M))
                : 0M;

            var pct = (s.GoalCash.Value == 0) ? 0M : (raised / s.GoalCash.Value) * 100M;

            if (pct >= 100M) return "مكتملة";

            if (pct >= 75M
                && s.EndDate.HasValue
                && (s.EndDate.Value - DateTime.UtcNow).TotalDays <= 7)
                return "عاجلة";

            return "نشطة";
        }
    }
}