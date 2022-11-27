namespace Application.Common.Models;

public sealed class ErrorResponse
{
    public string[] Errors { get; set; } = null!;
    public string Type { get; set; } = null!;
}