using Api.Errors;
using ErrorOr;

namespace Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToOkResultOrProblem<TResponse>(this ErrorOr<TResponse> result, HttpContext context) =>
        result.MatchFirst(
            Results.Ok,
            error =>
                Results.Problem(
                    error.Type switch
                    {
                        ErrorType.Validation => Problems.Validation(context, error.Metadata),
                        ErrorType.Conflict => Problems.Conflict(context, error.Description),
                        ErrorType.NotFound => Problems.NotFound(context, error.Description),
                        ErrorType.Forbidden => Problems.Forbidden(context),
                        ErrorType.Unauthorized => Problems.Unauthorized(context),
                        _ => Problems.Failure(context, error.Description),
                    }
                )
        );
}
