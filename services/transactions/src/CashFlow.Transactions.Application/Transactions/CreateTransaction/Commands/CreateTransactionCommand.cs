using CashFlow.Shared.Domain.Enums;
using CashFlow.Transactions.Application.Common.Dtos;

namespace CashFlow.Transactions.Application.Transactions.CreateTransaction.Commands;

public record CreateTransactionCommand(
    decimal Amount,
    ETransactionType Type,
    string? Description) : IRequest<TransactionDto>;