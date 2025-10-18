using ErrorOr;

namespace TestFixtures;

public static class FixtureBase
{
    public static string GenerateLongString(int length) => new('a', length);

    public static void AssertError(ErrorOr<Success> result, Error expectedError)
    {
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(expectedError.Code);
        result.FirstError.Description.ShouldBe(expectedError.Description);
    }

    public static void AssertError<T>(ErrorOr<T> result, Error expectedError)
    {
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(expectedError.Code);
        result.FirstError.Description.ShouldBe(expectedError.Description);
    }

    public static void AssertSuccess(ErrorOr<Success> result) => result.IsError.ShouldBeFalse();

    public static DateTimeOffset GetTestDate(int daysOffset = 0) => DateTimeOffset.UtcNow.AddDays(daysOffset);
}
