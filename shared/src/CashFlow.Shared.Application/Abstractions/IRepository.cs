namespace CashFlow.Shared.Application.Abstractions;

public interface IRepository<T> : IRepositoryBase<T>, IReadRepositoryBase<T> where T : class { }

