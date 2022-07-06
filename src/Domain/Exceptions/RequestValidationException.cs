using System.Runtime.Serialization;

namespace Domain.Exceptions;

[Serializable]
public class RequestValidationException : Exception
{
    public RequestValidationException(string message) : base(message)
    {
    }

    protected RequestValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}