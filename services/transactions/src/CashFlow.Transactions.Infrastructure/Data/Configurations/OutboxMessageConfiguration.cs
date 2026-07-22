using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasColumnOrder(1)
            .IsRequired();

        builder.Property(i => i.QueueName)
            .HasColumnName("queue_name")
            .HasColumnOrder(2)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(i => i.Payload)
            .HasColumnName("payload")
            .HasColumnOrder(3)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(i => i.ErrorMessage)
            .HasColumnName("error_message")
            .HasColumnOrder(4)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(i => i.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnOrder(5)
            .IsRequired();

        builder.HasIndex(i => i.QueueName)
            .HasDatabaseName("ix_outbox_messages_queue_name");

        builder.HasIndex(i => i.CreatedAtUtc)
            .HasDatabaseName("ix_outbox_messages_created_at_utc");
    }
}
