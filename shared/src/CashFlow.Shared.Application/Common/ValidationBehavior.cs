namespace CashFlow.Shared.Application.Common;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            _logger.LogInformation("Running validation for {RequestType}", typeof(TRequest).Name);

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(i => i.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(i => i.Errors).Where(i => i != null).ToList();

            if (failures.Count > 0)
            {
                _logger.LogWarning(
                    "Validation failed for {RequestType}. Errors: {@Errors}",
                    typeof(TRequest).Name,
                    failures
                );

                throw new ValidationException(failures);
            }
        }

        return await next(cancellationToken);
    }
}

