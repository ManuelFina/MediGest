
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class MedicalServiceConfiguration : IEntityTypeConfiguration<MedicalService>
{
    public void Configure(EntityTypeBuilder<MedicalService> builder)
    {
        builder.ToTable("MedicalServices");

        builder.HasKey(ms => ms.Id);

        builder.Property(ms => ms.Code)
            .HasMaxLength(30);

        builder.Property(ms => ms.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(ms => ms.Description)
            .HasMaxLength(500);

        builder.Property(ms => ms.PrivatePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ms => ms.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Índice único para códigos informados.
        builder.HasIndex(ms => ms.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        // Búsqueda por nombre.
        builder.HasIndex(ms => ms.Name);

        // Relación con Specialty.
        builder.HasOne(ms => ms.Specialty)
            .WithMany(s => s.MedicalServices)
            .HasForeignKey(ms => ms.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
