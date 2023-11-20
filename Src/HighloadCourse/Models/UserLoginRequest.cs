using System.ComponentModel.DataAnnotations;

namespace HighloadCourse.Models;

public sealed class UserLoginRequest
{
    [Required]
    public required string Id { get; init; }

    [Required]
    public required string Password { get; init; }
}