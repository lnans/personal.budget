using System.Runtime.Serialization;

namespace Domain.Exceptions;

[Serializable]
public class AlreadyExistException : Exception
{
    public AlreadyExistException(string message) : base(message)
    {
    }

    protected AlreadyExistException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}