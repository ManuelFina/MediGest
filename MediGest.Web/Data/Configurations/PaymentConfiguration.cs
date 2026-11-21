
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasPrecision(15, 2)
            .IsRequired();

        builder.Property(p => p.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Reference)
            .HasMaxLength(100);

        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.InvoiceId);
    }
}
