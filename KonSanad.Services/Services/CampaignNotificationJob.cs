using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KonSanad.Services.BackgroundJobs
{
    public class CampaignNotificationJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CampaignNotificationJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var context = scope.ServiceProvider
                        .GetRequiredService<KonSanadDbContext>();

                    var notificationService = scope.ServiceProvider
                        .GetRequiredService<INotificationService>();

                    var today = DateTime.UtcNow.Date;

                    // =====================================================
                    // 1) الحملة قربت تنتهي
                    // =====================================================

                    var nearEndCampaigns = await context.Campaigns
                        .Include(c => c.CampaignVolunteers)
                        .Include(c => c.BeneficiaryCampaigns)
                        .Where(c =>
                            !c.IsDeleted &&
                            !c.IsNearEndDateNotificationSent &&
                            c.EndDate.HasValue &&
                            c.EndDate.Value.Date <= today.AddDays(2) &&
                            c.EndDate.Value.Date > today)
                        .ToListAsync(stoppingToken);

                    foreach (var campaign in nearEndCampaigns)
                    {
                        if (campaign.ManagerId != null)
                        {
                            await notificationService.NotifyUserAsync(
                                campaign.ManagerId.ToString(),
                                new NotificationMessageDto
                                {
                                    Title = "الحملة قربت تنتهي",
                                    Message = $"الحملة {campaign.Title} هتنتهي خلال يومين",
                                    //Type = Campaign,
                                    EntityId = campaign.Id,
                                    EntityType = nameof(Campaign),
                                    CreatedAtUtc = DateTime.UtcNow
                                });
                        }

                        campaign.IsNearEndDateNotificationSent = true;
                    }

                    // =====================================================
                    // 2) الحملة انتهت
                    // =====================================================

                    var endedCampaigns = await context.Campaigns
                        .Where(c =>
                            !c.IsDeleted &&
                            !c.IsEndDateNotificationSent &&
                            c.EndDate.HasValue &&
                            c.EndDate.Value.Date <= today)
                        .ToListAsync(stoppingToken);

                    foreach (var campaign in endedCampaigns)
                    {
                        if (campaign.ManagerId != null)
                        {
                            await notificationService.NotifyUserAsync(
                                campaign.ManagerId.ToString(),
                                new NotificationMessageDto
                                {
                                    Title = "انتهاء الحملة",
                                    Message = $"الحملة {campaign.Title} انتهت",
                                   // Type = "Campaign",
                                    EntityId = campaign.Id,
                                    EntityType = nameof(Campaign),
                                    CreatedAtUtc = DateTime.UtcNow
                                });
                        }

                        campaign.IsEndDateNotificationSent = true;
                    }

                    // =====================================================
                    // 3) الوصول للهدف المالي
                    // =====================================================

                    var completedGoals = await context.Campaigns
                        .Include(c => c.DonationOrders)
                        .Where(c =>
                            !c.IsDeleted &&
                            !c.IsGoalReachedNotificationSent &&
                            c.GoalCash.HasValue)
                        .ToListAsync(stoppingToken);

                    foreach (var campaign in completedGoals)
                    {
                        decimal totalDonations = campaign.DonationOrders
                            .Where(d => !d.IsDeleted)
                            .Sum(d => d.CashAmount ?? 0);

                        if (totalDonations >= campaign.GoalCash)
                        {
                            if (campaign.ManagerId != null)
                            {
                                await notificationService.NotifyUserAsync(
                                    campaign.ManagerId.ToString(),
                                    new NotificationMessageDto
                                    {
                                        Title = "تم اكتمال الهدف المالي",
                                        Message = $"تم الوصول للهدف المالي للحملة {campaign.Title}",
                                       // Type = "Campaign",
                                        EntityId = campaign.Id,
                                        EntityType = nameof(Campaign),
                                        CreatedAtUtc = DateTime.UtcNow
                                    });
                            }

                            campaign.IsGoalReachedNotificationSent = true;
                        }
                    }

                    // =====================================================
                    // 4) اكتمال عدد المستفيدين
                    // =====================================================

                    var beneficiariesCompleted = await context.Campaigns
                        .Include(c => c.BeneficiaryCampaigns)
                        .Where(c =>
                            !c.IsDeleted &&
                            !c.IsBeneficiariesCompletedNotificationSent)
                        .ToListAsync(stoppingToken);

                    foreach (var campaign in beneficiariesCompleted)
                    {
                        int beneficiariesCount = campaign.BeneficiaryCampaigns.Count;

                        if (beneficiariesCount >= campaign.MaxBeneficiaries)
                        {
                            if (campaign.ManagerId != null)
                            {
                                await notificationService.NotifyUserAsync(
                                    campaign.ManagerId.ToString(),
                                    new NotificationMessageDto
                                    {
                                        Title = "اكتمل عدد المستفيدين",
                                        Message = $"اكتمل عدد المستفيدين في حملة {campaign.Title}",
                                       // Type = "Campaign",
                                        EntityId = campaign.Id,
                                        EntityType = nameof(Campaign),
                                        CreatedAtUtc = DateTime.UtcNow
                                    });
                            }

                            campaign.IsBeneficiariesCompletedNotificationSent = true;
                        }
                    }

                    // =====================================================
                    // 5) اكتمال عدد المتطوعين
                    // =====================================================

                    var volunteersCompleted = await context.Campaigns
                        .Include(c => c.CampaignVolunteers)
                        .Where(c =>
                            !c.IsDeleted &&
                            !c.IsVolunteersCompletedNotificationSent)
                        .ToListAsync(stoppingToken);

                    foreach (var campaign in volunteersCompleted)
                    {
                        int volunteersCount = campaign.CampaignVolunteers.Count;

                        // غير الرقم ده حسب المطلوب عندك
                        int requiredVolunteers = 10;

                        if (volunteersCount >= requiredVolunteers)
                        {
                            if (campaign.ManagerId != null)
                            {
                                await notificationService.NotifyUserAsync(
                                    campaign.ManagerId.ToString(),
                                    new NotificationMessageDto
                                    {
                                        Title = "اكتمل عدد المتطوعين",
                                        Message = $"تم اكتمال عدد المتطوعين في حملة {campaign.Title}",
                                       // Type = "Campaign",
                                        EntityId = campaign.Id,
                                        EntityType = nameof(Campaign),
                                        CreatedAtUtc = DateTime.UtcNow
                                    });
                            }

                            campaign.IsVolunteersCompletedNotificationSent = true;
                        }
                    }
                    // =====================================================
                    // 6) المهمة قربت تنتهي
                    // =====================================================

                    var nearDeadlineMissions = await context.Missions
     .Include(m => m.VolunteerMissions)
         .ThenInclude(vm => vm.Volunteer)
     .Where(m =>
         !m.IsDeleted &&
         !m.IsNearDeadlineNotificationSent &&
         m.EndDate.HasValue &&
         m.EndDate.Value.Date <= today.AddDays(1) &&
         m.EndDate.Value.Date > today)
     .ToListAsync(stoppingToken);

                    foreach (var mission in nearDeadlineMissions)
                    {
                        var volunteers = mission.VolunteerMissions
                            .Select(vm => vm.Volunteer)
                            .Where(v => v != null && !string.IsNullOrEmpty(v.AppUserId))
                            .ToList();

                        foreach (var volunteer in volunteers)
                        {
                            await notificationService.NotifyUserAsync(
                                volunteer.AppUserId!,
                                new NotificationMessageDto
                                {
                                    Title = "المهمة قربت تنتهي",
                                    Message = $"المهمة {mission.Title} هتنتهي بكرة",
                                    EntityId = mission.Id,
                                    EntityType = nameof(Mission),
                                    CreatedAtUtc = DateTime.UtcNow
                                });
                        }

                        mission.IsNearDeadlineNotificationSent = true;
                    }





                    // =====================================================
                    // 7) المهمة انتهت
                    // =====================================================




                    var endedMissions = await context.Missions
    .Include(m => m.VolunteerMissions)
        .ThenInclude(vm => vm.Volunteer)
    .Where(m =>
        !m.IsDeleted &&
        !m.IsDeadlinePassedNotificationSent &&
        m.EndDate.HasValue &&
        m.EndDate.Value.Date <= today)
    .ToListAsync(stoppingToken);

                    foreach (var mission in endedMissions)
                    {
                        var volunteers = mission.VolunteerMissions
                            .Select(vm => vm.Volunteer)
                            .Where(v => v != null && !string.IsNullOrEmpty(v.AppUserId))
                            .ToList();

                        foreach (var volunteer in volunteers)
                        {
                            await notificationService.NotifyUserAsync(
                                volunteer.AppUserId!,
                                new NotificationMessageDto
                                {
                                    Title = "انتهاء المهمة",
                                    Message = $"المهمة {mission.Title} انتهت",
                                    EntityId = mission.Id,
                                    EntityType = nameof(Mission),
                                    CreatedAtUtc = DateTime.UtcNow
                                });
                        }

                        mission.IsDeadlinePassedNotificationSent = true;
                    }
                    // =====================================================


                    // Save Changes
                    // =====================================================

                    await context.SaveChangesAsync(stoppingToken);

                    // =====================================================
                    // Delay 1 Hour
                    // =====================================================

                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }

            }
        }
    }
}