
using MediGest.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediGest.Web.Data.Configurations;

public class ScheduleDayConfiguration
    : IEntityTypeConfiguration<ScheduleDay>
{
    public void Configure(EntityTypeBuilder<ScheduleDay> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DayOfWeek)
            .IsRequired();

        builder.Property(d => d.StartTime)
            .IsRequired();

        builder.Property(d => d.EndTime)
            .IsRequired();

        builder.HasIndex(d => new
        {
            d.ProfessionalScheduleId,
            d.DayOfWeek,
            d.StartTime
        });
    }
}
