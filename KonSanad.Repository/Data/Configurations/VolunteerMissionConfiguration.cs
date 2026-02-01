
﻿using KonSanad.Core.Entity;
using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Repository.Data.Configurations;


public class VolunteerMissionConfiguration : IEntityTypeConfiguration<MissionVolunteers>
{
    public void Configure(EntityTypeBuilder<MissionVolunteers> builder)
    {
        // Table
        builder.ToTable("MissionVolunteers");

        // Primary Key
        builder.HasKey(vm => vm.Id);

        // ----------------------------
        // Properties
        // ----------------------------
        builder.Ignore(x => x.Status);

        builder.Property(vm => vm.ResultMission)
               .HasMaxLength(1000)
               .IsRequired(false);
        builder.Property(vm => vm.MissionStatus)

             .IsRequired(true);
        builder.Property(x => x.CreatedAt).IsRequired();

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