using CashFlow.Domain.Entities;

namespace CashFlow.Application.Abstractions;

public interface IRepository<T> : IRepositoryBase<T>, IReadRepositoryBase<T> where T : BaseEntity { }

