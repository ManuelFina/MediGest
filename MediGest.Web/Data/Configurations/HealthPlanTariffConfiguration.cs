
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class HealthPlanTariffConfiguration
    : IEntityTypeConfiguration<HealthPlanTariff>
{
    public void Configure(EntityTypeBuilder<HealthPlanTariff> builder)
    {
        builder.ToTable("HealthPlanTariffs");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.ValidFrom)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(t => t.ValidTo)
            .HasColumnType("date");

        builder.Property(t => t.RequiresAuthorization)
            .IsRequired();

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        // Relación con el plan de cobertura.
        builder.HasOne(t => t.HealthPlan)
            .WithMany(p => p.Tariffs)
            .HasForeignKey(t => t.HealthPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con la prestación médica.
        builder.HasOne(t => t.MedicalService)
            .WithMany(s => s.Tariffs)
            .HasForeignKey(t => t.MedicalServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice para consultar tarifas por plan y prestación.
        builder.HasIndex(t => new
        {
            t.HealthPlanId,
            t.MedicalServiceId,
            t.ValidFrom
        });
    }
}
