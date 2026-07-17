using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(om => om.Id);

        builder.Property(om => om.Id)
            .ValueGeneratedNever();

        builder.Property(om => om.QueueName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(om => om.Payload)
            .HasColumnType("jsonb") // PostgreSQL JSONB
            .IsRequired();

        builder.Property(om => om.ErrorMessage)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(om => om.CreatedAtUtc)
            .IsRequired();

        // Índice para o worker buscar mensagens por fila
        builder.HasIndex(om => om.QueueName)
            .HasDatabaseName("IX_OutboxMessage_QueueName");

        // Índice para limpeza periódica (mensagens antigas processadas)
        builder.HasIndex(om => om.CreatedAtUtc)
            .HasDatabaseName("IX_OutboxMessage_CreatedAtUtc");
    }
}
