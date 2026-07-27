using CashFlow.Consolidation.Application.Abstractions;
using CashFlow.Consolidation.Domain.Entities;

namespace CashFlow.Consolidation.Infrastructure.Data.Repositories;

public class ProcessedTransactionRepository : Repository<ProcessedTransaction>, IProcessedTransactionRepository
{
    public ProcessedTransactionRepository(ApplicationDbContext dbContext) : base(dbContext) { }
}
