using CashFlow.Shared.Application.Specifications;
using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Application.Transactions.ListTransactions.Specifications
{
    public class ListTransactionsSpecification : PaginatedListSpecification<Transaction>
    {
        public ListTransactionsSpecification(
            DateTimeOffset? startDate,
            DateTimeOffset? endDate,
            int pageNumber,
            int pageSize) : base(pageNumber, pageSize)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                var startOfDay = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                var endOfNextDay = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero);

                Query.Where(i => i.CreatedAtUtc >= startOfDay && i.CreatedAtUtc < endOfNextDay);
            }
            else if (startDate.HasValue)
            {
                var startOfDay = new DateTimeOffset(startDate.Value.Date, TimeSpan.Zero);
                Query.Where(i => i.CreatedAtUtc >= startOfDay);
            }
            else if (endDate.HasValue)
            {
                var endOfNextDay = new DateTimeOffset(endDate.Value.Date.AddDays(1), TimeSpan.Zero);
                Query.Where(i => i.CreatedAtUtc < endOfNextDay);
            }

            Query.OrderByDescending(i => i.CreatedAtUtc);
        }
    }
}
