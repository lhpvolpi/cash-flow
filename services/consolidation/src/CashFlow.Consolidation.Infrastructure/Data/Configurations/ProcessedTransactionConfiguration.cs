using CashFlow.Consolidation.Domain.Entities;

namespace CashFlow.Consolidation.Infrastructure.Data.Configurations;

public class ProcessedTransactionConfiguration : IEntityTypeConfiguration<ProcessedTransaction>
{
    public void Configure(EntityTypeBuilder<ProcessedTransaction> builder)
    {
        builder.ToTable("processed_transactions");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasColumnOrder(1)
            .IsRequired();

        builder.Property(e => e.TransactionId)
            .HasColumnName("transaction_id")
            .HasColumnOrder(2)
            .IsRequired();

        builder.Property(e => e.DailyBalanceId)
            .HasColumnName("daily_balance_id")
            .HasColumnOrder(3)
            .IsRequired();

        builder.HasIndex(e => e.TransactionId)
            .HasDatabaseName("ix_processed_transaction_transaction_id")
            .IsUnique();

        builder.HasOne<DailyBalance>()
            .WithMany()
            .HasForeignKey(e => e.DailyBalanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
