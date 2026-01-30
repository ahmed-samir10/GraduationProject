using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Repository.Data.Configurations;

public class VolunteerMissionConfiguration : IEntityTypeConfiguration<VolunteerMission>
{
    public void Configure(EntityTypeBuilder<VolunteerMission> builder)
    {
        // Table
        builder.ToTable("VolunteerMission");

        // Primary Key
        builder.HasKey(vm => vm.Id);

        // ----------------------------
        // Properties
        // ----------------------------
        builder.Property(vm => vm.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(vm => vm.Description)
               .HasMaxLength(1000)
               .IsRequired(false);

        builder.Property(vm => vm.Address)
               .HasMaxLength(300)
               .IsRequired(false);

        builder.Property(vm => vm.PhoneNumber)
               .HasMaxLength(20)
               .IsRequired(false);

        builder.Property(vm => vm.ResultMission)
               .HasMaxLength(1000)
               .IsRequired(false);

        builder.Property(vm => vm.StartDate)
               .IsRequired(false);

        builder.Property(vm => vm.EndDate)
               .IsRequired(false);

        // ----------------------------
        // Relationships
        // ----------------------------

        // Volunteer (One Volunteer → Many VolunteerMissions)
        builder.HasOne(vm => vm.Volunteer)
               .WithMany(v => v.VolunteerMissions)
               .HasForeignKey(vm => vm.VolunteerId)
               .OnDelete(DeleteBehavior.Restrict);

        // Mission (One Mission → Many VolunteerMissions)
        builder.HasOne(vm => vm.Mission)
               .WithMany(m => m.VolunteerMissions)
               .HasForeignKey(vm => vm.MissionId)
               .OnDelete(DeleteBehavior.Restrict);

        // ----------------------------
        // Indexes
        // ----------------------------

        // Prevent duplicate assignment of the same volunteer to the same mission
        builder.HasIndex(vm => new { vm.VolunteerId, vm.MissionId })
               .IsUnique();
    }
}