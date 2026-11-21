
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class HealthInsuranceConfiguration
    : IEntityTypeConfiguration<HealthInsurance>
{
    public void Configure(EntityTypeBuilder<HealthInsurance> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.LegalName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.CUIT)
            .IsRequired()
            .HasMaxLength(11);

        builder.HasIndex(h => h.CUIT)
            .IsUnique();

        builder.Property(h => h.EntityType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.RegistrationNumber)
            .HasMaxLength(30);

        builder.Property(h => h.PhoneNumber)
            .HasMaxLength(25);

        builder.Property(h => h.Email)
            .HasMaxLength(150);

        builder.Property(h => h.Address)
            .HasMaxLength(200);

        builder.Property(h => h.Notes)
            .HasMaxLength(500);

        builder.Property(h => h.IsActive)
            .IsRequired();

        builder.HasMany(h => h.Plans)
            .WithOne(p => p.HealthInsurance)
            .HasForeignKey(p => p.HealthInsuranceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
