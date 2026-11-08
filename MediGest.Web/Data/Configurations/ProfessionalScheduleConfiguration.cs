
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class ProfessionalScheduleConfiguration
    : IEntityTypeConfiguration<ProfessionalSchedule>
{
    public void Configure(EntityTypeBuilder<ProfessionalSchedule> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.StartDate)
            .IsRequired();

        builder.Property(s => s.EndDate)
            .IsRequired();

        builder.Property(s => s.AppointmentDurationMinutes)
            .IsRequired();

        builder.HasMany(s => s.ScheduleDays)
            .WithOne(d => d.ProfessionalSchedule)
            .HasForeignKey(d => d.ProfessionalScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new
        {
            s.ProfessionalId,
            s.StartDate,
            s.EndDate
        });
    }
}
