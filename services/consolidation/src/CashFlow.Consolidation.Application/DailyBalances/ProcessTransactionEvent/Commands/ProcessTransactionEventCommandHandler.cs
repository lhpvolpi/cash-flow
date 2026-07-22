namespace CashFlow.Consolidation.Application.DailyBalances.ProcessTransactionEvent.Commands;

public class ProcessTransactionEventCommandHandler : IRequestHandler<ProcessTransactionEventCommand>
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessTransactionEventCommandHandler(
        IDailyBalanceRepository dailyBalanceRepository,
        IUnitOfWork unitOfWork)
    {
        _dailyBalanceRepository = dailyBalanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessTransactionEventCommand request, CancellationToken cancellationToken)
    {
        var date = DateOnly.FromDateTime(request.OccurredAtUtc.DateTime);

        var specification = new GetDailyBalanceSpecification(date);
        var dailyBalance = await _dailyBalanceRepository.FirstOrDefaultAsync(specification, cancellationToken);

        try
        {
            if (dailyBalance is null)
            {
                dailyBalance = new DailyBalance(date);
                await _dailyBalanceRepository.AddAsync(dailyBalance, cancellationToken);
            }

            dailyBalance.Apply(request.TransactionType, request.Amount);

            await _unitOfWork.CommitAsync(cancellationToken);

        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
