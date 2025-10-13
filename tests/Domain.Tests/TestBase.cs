using ErrorOr;

namespace Domain.Tests;

public abstract class TestBase
{
    protected static string GenerateLongString(int length) => new('a', length);

    protected static void AssertError(
        ErrorOr<Success> result,
        Error expectedError
    )
    {
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(expectedError.Code);
        result.FirstError.Description.ShouldBe(expectedError.Description);
    }

    protected static void AssertError<T>(ErrorOr<T> result, Error expectedError)
    {
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(expectedError.Code);
        result.FirstError.Description.ShouldBe(expectedError.Description);
    }

    protected static void AssertSuccess(ErrorOr<Success> result) =>
        result.IsError.ShouldBeFalse();

    protected static DateTimeOffset GetTestDate(int daysOffset = 0) =>
        DateTimeOffset.UtcNow.AddDays(daysOffset);
}
