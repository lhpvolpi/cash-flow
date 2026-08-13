namespace CashFlow.Auth.Application.Common.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid username or password.")
    {
    }
}
