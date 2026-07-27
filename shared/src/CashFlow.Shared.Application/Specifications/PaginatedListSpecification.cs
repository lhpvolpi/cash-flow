namespace CashFlow.Shared.Application.Specifications;

public class PaginatedListSpecification<TEntity> : Specification<TEntity> where TEntity : class
{
    public PaginatedListSpecification(int pageNumber = 1, int pageSize = 10)
        => Query.AsNoTracking().Skip((pageNumber - 1) * pageSize).Take(pageSize);
}

