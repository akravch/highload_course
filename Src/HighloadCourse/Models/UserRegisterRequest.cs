using System.ComponentModel.DataAnnotations;

namespace HighloadCourse.Models;

public sealed class UserRegisterRequest
{
    [Required]
    [MaxLength(128)]
    public required string FirstName { get; init; }

    [Required]
    [MaxLength(128)]
    public required string SecondName { get; init; }

    [Required]
    [MaxLength(2048)]
    public required string Biography { get; init; }

    [Required]
    [MaxLength(128)]
    public required string City { get; init; }

    [Required]
    public required string Password { get; init; }

    [Required]
    public required DateOnly Birthdate { get; init; }
}