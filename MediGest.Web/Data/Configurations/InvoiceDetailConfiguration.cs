
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
    {
        builder.ToTable("InvoiceDetails");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Quantity)
            .IsRequired();

        builder.Property(d => d.UnitPrice)
            .HasPrecision(15, 2);

        builder.Property(d => d.UnitCoverageAmount)
            .HasPrecision(15, 2);

        builder.Property(d => d.Subtotal)
            .HasPrecision(15, 2);

        builder.Property(d => d.CoverageAmount)
            .HasPrecision(15, 2);

        builder.Property(d => d.PatientAmount)
            .HasPrecision(15, 2);

        builder.HasOne(d => d.Invoice)
            .WithMany(i => i.Details)
            .HasForeignKey(d => d.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.MedicalService)
            .WithMany()
            .HasForeignKey(d => d.MedicalServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Appointment)
            .WithMany()
            .HasForeignKey(d => d.AppointmentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
