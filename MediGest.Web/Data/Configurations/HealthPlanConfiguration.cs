
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class HealthPlanConfiguration
    : IEntityTypeConfiguration<HealthPlan>
{
    public void Configure(EntityTypeBuilder<HealthPlan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Code)
            .HasMaxLength(30);

        builder.Property(p => p.Description)
            .HasMaxLength(300);

        builder.Property(p => p.RequiresAuthorization)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        // El código debe ser único dentro de cada cobertura.
        // Se permiten múltiples planes sin código (NULL).
        builder.HasIndex(p => new
        {
            p.HealthInsuranceId,
            p.Code
        })
        .IsUnique()
        .HasFilter("[Code] IS NOT NULL");

        builder.HasOne(p => p.HealthInsurance)
            .WithMany(h => h.Plans)
            .HasForeignKey(p => p.HealthInsuranceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
