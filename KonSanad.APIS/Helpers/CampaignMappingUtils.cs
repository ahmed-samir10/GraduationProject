using System;
using System.Linq;
using KonSanad.Repository.Entities;

namespace KonSanad.APIS.Helpers
{
    /// <summary>
    /// Utility methods to compute UI-friendly values for Campaign listing/cards:
    /// - CalculateProgress: returns integer percentage 0..100 of cash goal reached
    /// - CalculateStatus: returns a small status string ("نشطة", "عاجلة", "مكتملة")
    /// 
    /// Comments inside explain intent and edge-cases so you can understand every line.
    /// </summary>
    public static class CampaignMappingUtils
    {
        /// <summary>
        /// Calculate cash progress percentage for a campaign.
        /// Returns an int between 0 and 100.
        /// Explanation:
        /// - If no GoalCash or goal <= 0 => progress is 0 (cannot compute percentage).
        /// - Sum cash raised from DonationOrders. Each donation may store cash in two places:
        ///   - DonationOrder.CashAmount (nullable), or
        ///   - DonationOrder.CashDonation.Amount (if CashDonation navigation exists).
        ///   We use CashAmount when present; otherwise, fall back to CashDonation.Amount if provided.
        /// - Compute pct = (raised / GoalCash) * 100.
        /// - Clamp pct into [0,100] to avoid negative or >100 values.
        /// - Cast to int for UI display (drops fractional part).
        /// </summary>
        public static int CalculateProgress(Campaign s)
        {
            // Defensive: if campaign has no cash goal, we cannot compute meaningful progress
            if (!s.GoalCash.HasValue || s.GoalCash.Value <= 0) return 0;

            // Sum all cash donations related to this campaign.
            // Use CashAmount when set; otherwise, use linked CashDonation.Amount if present.
            var raised = s.DonationOrders != null
                ? s.DonationOrders.Sum(dono =>
                    // dono.CashAmount is nullable decimal?,
                    // if it has a value use it, otherwise check dono.CashDonation.Amount
                    dono.CashAmount ?? (dono.CashDonation != null ? dono.CashDonation.Amount : 0M))
                : 0M;

            // Compute percentage as decimal to preserve precision during calculation.
            var pct = (raised / s.GoalCash.Value) * 100M;

            // Clamp: ensure percentage is within 0..100 range.
            if (pct > 100M) pct = 100M;
            if (pct < 0M) pct = 0M;

            // Return integer percent (UI typically shows whole numbers)
            return (int)pct;
        }

        /// <summary>
        /// Calculate a short status string for the campaign for UI cards.
        /// Rules implemented:
        /// - If no GoalCash or goal <= 0 => return "نشطة" (active) — no cash goal to evaluate urgency.
        /// - Compute percentage using same summation logic as CalculateProgress.
        /// - If percentage >= 100 => "مكتملة" (completed).
        /// - If percentage >= 75 AND campaign EndDate is within 7 days from now => "عاجلة" (urgent).
        /// - Otherwise => "نشطة" (active).
        /// 
        /// Notes:
        /// - Uses UTC via DateTime.UtcNow to avoid timezone skew in server environments.
        /// - This is purely presentation logic; adjust thresholds or labels as product needs change.
        /// </summary>
        public static string CalculateStatus(Campaign s)
        {
            // If there is no valid cash target, we treat campaign as active (not urgent/completed)
            if (!s.GoalCash.HasValue || s.GoalCash.Value <= 0)
                return "نشطة";

            // Same raised calculation as in CalculateProgress
            var raised = s.DonationOrders != null
                ? s.DonationOrders.Sum(dono =>
                    dono.CashAmount ?? (dono.CashDonation != null ? dono.CashDonation.Amount : 0M))
                : 0M;

            // Avoid division by zero; if GoalCash is zero treat as 0%
            var pct = (s.GoalCash.Value == 0) ? 0M : (raised / s.GoalCash.Value) * 100M;

            // Completed if reached or exceeded 100%
            if (pct >= 100M) return "مكتملة";

            // Urgent if at least 75% and the campaign ends within the next 7 days
            if (pct >= 75M
                && s.EndDate.HasValue
                && (s.EndDate.Value - DateTime.UtcNow).TotalDays <= 7)
                return "عاجلة";

            // Default: active
            return "نشطة";
        }
    }
}
