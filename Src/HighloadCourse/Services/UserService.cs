using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HighloadCourse.Models;
using HighloadCourse.Utils;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using NpgsqlTypes;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace HighloadCourse.Services;

public sealed class UserService : IDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly byte[] _authenticationKey;

    public UserService(IConfiguration configuration)
    {
        _dataSource = NpgsqlDataSource.Create(configuration["ConnectionString"]!);
        _authenticationKey = Encoding.UTF8.GetBytes(configuration["AuthenticationKey"]!);
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }

    public async Task<UserLoginResponse?> LoginAsync(UserLoginRequest request)
    {
        const string sql = "SELECT hash, salt FROM account WHERE id = $1";

        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddPositional(long.Parse(request.Id), NpgsqlDbType.Bigint);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        var storedHash = Convert.FromHexString(reader.GetString(0));
        var salt = Convert.FromHexString(reader.GetString(1));
        var hash = KeyDerivation.Pbkdf2(request.Password, salt, KeyDerivationPrf.HMACSHA256, 100_000, 64);

        if (!CryptographicOperations.FixedTimeEquals(hash, storedHash))
        {
            return null;
        }

        var key = new SymmetricSecurityKey(_authenticationKey);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, request.Id) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(descriptor);

        return new UserLoginResponse { Token = tokenHandler.WriteToken(token) };

    }

    public async Task<UserRegisterResponse> RegisterAsync(UserRegisterRequest request)
    {
        const string sql =
            """
            WITH account_insert AS (
                INSERT INTO account (hash, salt)
                VALUES (($1), ($2))
                RETURNING id
            )
            INSERT INTO account_info (account_id, first_name, second_name, biography, city, birthdate)
            VALUES ((SELECT id FROM account_insert), ($3), ($4), ($5), ($6), ($7))
            RETURNING (SELECT id FROM account_insert)
            """;

        await using var command = _dataSource.CreateCommand(sql);

        var salt = RandomNumberGenerator.GetBytes(32);
        var hash = KeyDerivation.Pbkdf2(request.Password, salt, KeyDerivationPrf.HMACSHA256, 100_000, 64);

        command.Parameters
            .AddPositional(Convert.ToHexString(hash), NpgsqlDbType.Char, 128)
            .AddPositional(Convert.ToHexString(salt), NpgsqlDbType.Char, 64)
            .AddPositional(request.FirstName, NpgsqlDbType.Varchar, 128)
            .AddPositional(request.SecondName, NpgsqlDbType.Varchar, 128)
            .AddPositional(request.Biography, NpgsqlDbType.Varchar, 2048)
            .AddPositional(request.City, NpgsqlDbType.Varchar, 128)
            .AddPositional(request.Birthdate, NpgsqlDbType.Date);

        var userId = (long) (await command.ExecuteScalarAsync())!;

        return new UserRegisterResponse { UserId = userId.ToString() };
    }

    public async Task<UserGetResponse?> GetAsync(string id)
    {
        const string sql = "SELECT first_name, second_name, biography, city, birthdate FROM account_info WHERE account_id = ($1) LIMIT 1";
        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddPositional(long.Parse(id), NpgsqlDbType.Bigint);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new UserGetResponse
        {
            Id = id,
            FirstName = reader.GetString(0),
            SecondName = reader.GetString(1),
            Biography = reader.GetString(2),
            City = reader.GetString(3),
            Birthdate = reader.GetFieldValue<DateOnly>(4)
        };
    }
}