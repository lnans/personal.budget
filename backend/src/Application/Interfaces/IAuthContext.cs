namespace Application.Interfaces;

public interface IAuthContext
{
    Guid CurrentUserId { get; }
}
