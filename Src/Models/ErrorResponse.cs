namespace HighloadCourse.Models;

public sealed class ErrorResponse
{
    public required string Message { get; init; }
    public required string RequestId { get; init; }
    public required int Code { get; init; }
}