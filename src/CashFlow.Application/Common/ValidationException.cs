namespace CashFlow.Application.Common;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more validation failures have occurred.")
        => Errors = new Dictionary<string, string[]>();

    public ValidationException(IEnumerable<ValidationFailure> failures) : base("One or more validation failures have occurred.")
        => Errors = failures
            .GroupBy(i => i.PropertyName, i => i.ErrorMessage)
            .ToDictionary(i => i.Key, i => i.ToArray());
}

