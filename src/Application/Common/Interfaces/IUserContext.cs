namespace Application.Common.Interfaces;

public interface IUserContext
{
    /// <summary>
    ///     Return the authenticated user id from the current http context
    /// </summary>
    /// <returns>user id</returns>
    string GetAuthenticatedUserId();
}