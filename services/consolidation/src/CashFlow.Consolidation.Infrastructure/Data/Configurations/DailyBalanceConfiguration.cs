using CashFlow.Consolidation.Domain.Entities;

namespace CashFlow.Consolidation.Infrastructure.Data.Configurations;

public class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
{
    public void Configure(EntityTypeBuilder<DailyBalance> builder)
    {
        builder.ToTable("daily_balances");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasColumnOrder(1)
            .IsRequired();

        builder.Property(e => e.Date)
            .HasColumnName("date")
            .HasColumnOrder(2)
            .IsRequired();

        builder.Property(e => e.TotalCredits)
            .HasColumnName("total_credits")
            .HasColumnOrder(3)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.TotalDebits)
            .HasColumnName("total_debits")
            .HasColumnOrder(4)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(e => e.Date)
            .HasDatabaseName("ix_daily_balance_date")
            .IsUnique();
    }
}
