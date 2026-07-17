using CashFlow.Application.Abstractions;
using CashFlow.Application.Common;
using CashFlow.Application.Common.Events;
using CashFlow.Domain.Entities;

namespace CashFlow.Application.Entries.CreateEntry.Commands;

public class CreateEntryCommandHandler : IRequestHandler<CreateEntryCommand, CreateEntryCommandResult>
{
    private readonly IEntryRepository _entryRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEntryCommandHandler(
        IEntryRepository entryRepository,
        IOutboxMessageRepository outboxMessageRepository,
        IUnitOfWork unitOfWork)
    {
        _entryRepository = entryRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateEntryCommandResult> Handle(CreateEntryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entry = new Entry(request.Amount, request.Type, request.Description);
            var outboxMessage = CreateOutboxMessage(entry);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            await _entryRepository.AddAsync(entry, cancellationToken);
            await _outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return new CreateEntryCommandResult(
                entry.Id,
                entry.Amount,
                entry.Type,
                entry.Description,
                entry.CreatedAtUtc);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static OutboxMessage CreateOutboxMessage(Entry entry)
    {
        var entryOperationEventPayload = new EntryOperationEventPayload(
            entry.Id,
            nameof(EEntryOperationEventType.EntryCreated),
            DateTimeOffset.UtcNow);

        var jsonDocument = entryOperationEventPayload.ToJsonDocument();
        var outboxMessage = new OutboxMessage("daily-balance-updates", jsonDocument);

        return outboxMessage;
    }
}