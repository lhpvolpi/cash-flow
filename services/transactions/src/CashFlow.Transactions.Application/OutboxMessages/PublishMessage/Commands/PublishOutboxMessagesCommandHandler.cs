using CashFlow.Shared.Application.Abstractions;
using CashFlow.Shared.Application.Models;
using CashFlow.Transactions.Application.Abstractions;

namespace CashFlow.Transactions.Application.OutboxMessages.PublishMessage.Commands;

public class PublishOutboxMessagesCommandHandler : IRequestHandler<PublishOutboxMessagesCommand>
{
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxBrokerPublisher _outboxMessagePublisher;

    public PublishOutboxMessagesCommandHandler(
        IOutboxMessageRepository outboxMessageRepository,
        IUnitOfWork unitOfWork,
        IOutboxBrokerPublisher outboxMessagePublisher)
    {
        _outboxMessageRepository = outboxMessageRepository;
        _unitOfWork = unitOfWork;
        _outboxMessagePublisher = outboxMessagePublisher;
    }

    public async Task Handle(PublishOutboxMessagesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var outboxMessages = await _outboxMessageRepository.GetNextBatchAsync(request.BatchSize, cancellationToken);

            foreach (var item in outboxMessages)
            {
                try
                {
                    var message = new BrokerMessage(item.Id, item.Payload, DateTimeOffset.UtcNow);

                    await _outboxMessagePublisher.SendMessageAsync(item.QueueName, message, cancellationToken);
                    await _outboxMessageRepository.DeleteAsync(item, cancellationToken);
                }
                catch (Exception ex)
                {
                    item.SetError(ex.Message);
                    await _outboxMessageRepository.UpdateAsync(item, cancellationToken);
                }
            }

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}


