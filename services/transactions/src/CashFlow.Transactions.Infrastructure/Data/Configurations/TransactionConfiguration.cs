using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {

        builder.ToTable("transactions");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasColumnOrder(1)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasColumnOrder(2)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasColumnName("type")
            .HasColumnOrder(3)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnOrder(4)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnOrder(5)
            .IsRequired();

        builder.HasIndex(e => new { e.CreatedAtUtc })
            .HasDatabaseName("ix_transaction_created_at_utc");
    }
}
