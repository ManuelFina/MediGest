
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.HasIndex(i => i.Number)
            .IsUnique();

        builder.Property(i => i.Number)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(i => i.IssueDate)
            .IsRequired();

        builder.Property(i => i.Subtotal)
            .HasPrecision(15, 2);

        builder.Property(i => i.CoverageAmount)
            .HasPrecision(15, 2);

        builder.Property(i => i.PatientAmount)
            .HasPrecision(15, 2);

        builder.Property(i => i.Total)
            .HasPrecision(15, 2);

        builder.Property(i => i.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.HasOne(i => i.Patient)
            .WithMany()
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.PatientAffiliation)
            .WithMany()
            .HasForeignKey(i => i.PatientAffiliationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Details)
            .WithOne(d => d.Invoice)
            .HasForeignKey(d => d.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
