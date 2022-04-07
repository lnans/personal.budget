namespace Domain.Exceptions;

[Serializable]
public class RequestValidationException : Exception
{
    public RequestValidationException(string message) : base(message)
    {
    }
}