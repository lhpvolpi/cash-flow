using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

public class ApplicationDbContextMigrator
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextMigrator> _logger;

    public ApplicationDbContextMigrator(
     ApplicationDbContext context,
     ILogger<ApplicationDbContextMigrator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ApplyMigrationsAsync()
    {
        try
        {
            if (!await _context.Database.CanConnectAsync())
            {
                _logger.LogWarning("Unable to connect to the database. Migration skipped.");
                return;
            }

            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while applying database migrations.");
            throw;
        }
    }
}