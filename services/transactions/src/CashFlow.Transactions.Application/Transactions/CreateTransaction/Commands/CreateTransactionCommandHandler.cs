using CashFlow.Shared.Application.Abstractions;
using CashFlow.Shared.Application.Common;
using CashFlow.Transactions.Application.Abstractions;
using CashFlow.Transactions.Application.Common.Dtos;
using CashFlow.Transactions.Application.Common.Events;
using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Application.Transactions.CreateTransaction.Commands;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IOutboxMessageRepository outboxMessageRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = new Transaction(request.Amount, request.Type, request.Description);
            var outboxMessage = CreateOutboxMessage(transaction);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return new TransactionDto(
                transaction.Id,
                transaction.Amount,
                transaction.Type,
                transaction.Description,
                transaction.CreatedAtUtc);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static OutboxMessage CreateOutboxMessage(Transaction transaction)
    {
        var transactionOperationEventPayload = new TransactionOperationEventPayload(
            transaction.Id,
            transaction.Amount,
            transaction.Type,
            ETransactionOperationEventType.TransactionCreated,
            DateTimeOffset.UtcNow);

        var jsonDocument = transactionOperationEventPayload.ToJsonDocument();
        var outboxMessage = new OutboxMessage("daily-balance-updates", jsonDocument);

        return outboxMessage;
    }
}