
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.LicenseNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(p => p.LicenseNumber)
            .IsUnique();

        builder.HasIndex(p => p.ApplicationUserId)
            .IsUnique();

        builder.HasOne(p => p.ApplicationUser)
            .WithOne()
            .HasForeignKey<Professional>(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Specialty relationship
        builder.HasOne(p => p.Specialty)
            .WithMany(s => s.Professionals)
            .HasForeignKey(p => p.SpecialtyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Schedules)
            .WithOne(s => s.Professional)
            .HasForeignKey(s => s.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.ScheduleExceptions)
            .WithOne(e => e.Professional)
            .HasForeignKey(e => e.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
