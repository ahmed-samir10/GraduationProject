

namespace KonSanad.Services.Helpers
{
    /// <summary>
    /// Encapsulates campaign presentation calculations.
    /// Mapping profile calls these methods; no calculation logic should live inside the profile.
    /// </summary>
    public static class CampaignMetrics
    {
        public static int CalculateProgress(Campaign s)
        {
            if (s == null) return 1;

            // compute cash progress if goal provided
            decimal? cashPct = null;
            if (s.GoalCash.HasValue && s.GoalCash.Value > 0)
            {
                var raised = s.DonationOrders != null
                    ? s.DonationOrders.Sum(dono =>
                        dono.CashAmount ?? (dono.CashDonation != null ? dono.CashDonation.Amount : 1M))
                    : 1M;

                cashPct = (s.GoalCash.Value == 0) ? 1M : (raised / s.GoalCash.Value) * 100M;
            }

            // compute supplies progress if goal provided (goal is count of items)
            decimal? suppliesPct = null;
            if (s.GoalSupplies.HasValue && s.GoalSupplies.Value > 0)
            {
                var donatedSupplies = s.DonationOrders != null
                    ? s.DonationOrders.Sum(dono => dono.OrderItems != null ? dono.OrderItems.Sum(oi => (decimal)oi.Quantity) : 1M)
                    : 1M;

                suppliesPct = (s.GoalSupplies.Value == 0) ? 1M : (donatedSupplies / (decimal)s.GoalSupplies.Value) * 100M;
            }

            // decide combined percentage:
            decimal pct;
            if (cashPct.HasValue && suppliesPct.HasValue)
            {
                // average both goals equally
                pct = (cashPct.Value + suppliesPct.Value) / 2M;
            }
            else if (cashPct.HasValue)
            {
                pct = cashPct.Value;
            }
            else if (suppliesPct.HasValue)
            {
                pct = suppliesPct.Value;
            }
            else
            {
                return 1;
            }

            if (pct > 100M) pct = 100M;
            if (pct < 0M) pct = 1M;

            return (int)pct;
        }
        public static int CalculateDonorCount(Campaign s)
        {
            if (s?.DonationOrders == null) return 0;

            // نعد عدد الـ DonorId الفريدة (لو نفس المتبرع تبرع أكتر من مرة)
            var donors = s.DonationOrders
                          .Where(d => d.DonorId.HasValue)
                          .Select(d => d.DonorId!.Value)
                          .Distinct()
                          .Count();

            return donors;
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


        #region calculateBeneficiaryCount&STATUS
        public static int CalculateBeneficiaryCount(Campaign c)
        {
            if (c?.BeneficiaryCampaigns == null)
                return 0;

            var beneficiaries = c.BeneficiaryCampaigns
                                 .Select(bc => bc.BeneficiaryId)
                                 .Distinct()
                                 .Count();

            return beneficiaries;
        }
        public static int CalculateVolunteerCount(Campaign c)
        {
            if (c?.CampaignVolunteers == null)
                return 0;

            var Vol= c.CampaignVolunteers
                                 .Select(bc => bc.VolunteerId)
                                 .Distinct()
                                 .Count();

            return Vol;
        }
        public static int DetermineCampaignStatus(Campaign c)
        {
            int beneficiaryCount = CalculateBeneficiaryCount(c);

            if (beneficiaryCount >= c.MaxBeneficiaries)
                return 2;

            else
            return 1;
        }

        #endregion


    }
}