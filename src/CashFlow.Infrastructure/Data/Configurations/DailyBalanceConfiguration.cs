using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Infrastructure.Data.Configurations;

public class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
{
    public void Configure(EntityTypeBuilder<DailyBalance> builder)
    {
        builder.ToTable("branches");
        builder.HasKey(db => db.Id);

        builder.Property(db => db.Id)
            .ValueGeneratedNever();

        builder.Property(db => db.Date)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(db => db.TotalCredits)
            .HasPrecision(18, 2)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(db => db.TotalDebits)
            .HasPrecision(18, 2)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(db => db.CreatedAtUtc)
            .IsRequired();

        // Índice único em Date para buscas rápidas (critério para 50 req/seg)
        builder.HasIndex(db => db.Date)
            .IsUnique()
            .HasDatabaseName("IX_DailyBalance_Date_Unique");
    }
}
