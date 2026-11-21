
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class PatientAffiliationConfiguration
    : IEntityTypeConfiguration<PatientAffiliation>
{
    public void Configure(EntityTypeBuilder<PatientAffiliation> builder)
    {
        builder.HasKey(affiliation => affiliation.Id);

        builder.Property(affiliation => affiliation.MembershipNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(affiliation => affiliation.MemberType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(affiliation => affiliation.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(affiliation => affiliation.ValidFrom)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(affiliation => affiliation.ValidTo)
            .HasColumnType("date");

        builder.Property(affiliation => affiliation.IsPrimary)
            .IsRequired();

        builder.Property(affiliation => affiliation.Notes)
            .HasMaxLength(500);

        // Relación con el paciente.
        builder.HasOne(affiliation => affiliation.Patient)
            .WithMany(patient => patient.Affiliations)
            .HasForeignKey(affiliation => affiliation.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con la cobertura médica.
        builder.HasOne(affiliation => affiliation.HealthInsurance)
            .WithMany(insurance => insurance.Affiliations)
            .HasForeignKey(affiliation => affiliation.HealthInsuranceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Clave alternativa para garantizar que el plan
        // pertenezca a la cobertura seleccionada.
        builder.HasOne(affiliation => affiliation.HealthPlan)
            .WithMany(plan => plan.Affiliations)
            .HasForeignKey(affiliation => new
            {
                affiliation.HealthInsuranceId,
                affiliation.HealthPlanId
            })
            .HasPrincipalKey(plan => new
            {
                plan.HealthInsuranceId,
                plan.Id
            })
            .OnDelete(DeleteBehavior.Restrict);

        // Índices para consultas frecuentes.
        builder.HasIndex(affiliation => affiliation.PatientId);

        builder.HasIndex(affiliation => affiliation.HealthInsuranceId);

        builder.HasIndex(affiliation => affiliation.HealthPlanId);

        builder.HasIndex(affiliation => new
        {
            affiliation.HealthInsuranceId,
            affiliation.MembershipNumber
        });
    }
}
