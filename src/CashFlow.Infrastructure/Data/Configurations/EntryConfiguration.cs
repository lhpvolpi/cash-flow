using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Infrastructure.Data.Configurations;

public class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        // Índice para buscar por data (importante para consolidação)
        builder.HasIndex(e => new { e.CreatedAtUtc })
            .HasDatabaseName("IX_Entry_CreatedAtUtc");
    }
}
