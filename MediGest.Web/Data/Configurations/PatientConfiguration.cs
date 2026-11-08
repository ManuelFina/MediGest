
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(p => p.DocumentNumber)
            .IsRequired()
            .HasMaxLength(15);

        builder.HasIndex(p => p.DocumentNumber)
            .IsUnique();

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(25);

        builder.Property(p => p.Email)
            .HasMaxLength(150);

        builder.Property(p => p.Address)
            .HasMaxLength(200);

        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
