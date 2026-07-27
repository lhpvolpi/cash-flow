namespace CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Commands;

public class ProcessTransactionEventCommandHandler : IRequestHandler<ProcessTransactionEventCommand>
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;
    private readonly IProcessedTransactionRepository _processedTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessTransactionEventCommandHandler(
        IDailyBalanceRepository dailyBalanceRepository,
        IProcessedTransactionRepository processedTransactionRepository,
        IUnitOfWork unitOfWork)
    {
        _dailyBalanceRepository = dailyBalanceRepository;
        _processedTransactionRepository = processedTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessTransactionEventCommand request, CancellationToken cancellationToken)
    {
        var alreadyProcessed = await _processedTransactionRepository.AnyAsync(
            new ProcessedTransactionByTransactionIdSpecification(request.TransactionId),
            cancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        var date = DateOnly.FromDateTime(request.OccurredAtUtc.DateTime);

        var specification = new GetDailyBalanceSpecification(date);
        var dailyBalance = await _dailyBalanceRepository.FirstOrDefaultAsync(specification, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            if (dailyBalance is null)
            {
                dailyBalance = new DailyBalance(date);
                await _dailyBalanceRepository.AddAsync(dailyBalance, cancellationToken);
            }

            dailyBalance.Apply(request.TransactionType, request.Amount);
            await _dailyBalanceRepository.UpdateAsync(dailyBalance, cancellationToken);

            var processedTransaction = new ProcessedTransaction(request.TransactionId, dailyBalance.Id);
            await _processedTransactionRepository.AddAsync(processedTransaction, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
