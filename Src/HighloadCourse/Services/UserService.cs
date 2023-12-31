﻿using System.IdentityModel.Tokens.Jwt;
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

public sealed class UserService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly byte[] _authenticationKey;

    public UserService(NpgsqlDataSource dataSource, IConfiguration configuration)
    {
        _dataSource = dataSource;
        _authenticationKey = Encoding.UTF8.GetBytes(configuration["AuthenticationKey"]!);
    }

    // success
    // not found
    // invalid password
    public async Task<UserLoginResult> LoginAsync(UserLoginRequest request)
    {
        const string sql = "SELECT hash, salt FROM account WHERE id = $1";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        if (!long.TryParse(request.Id, out var userId))
        {
            return UserLoginResult.NotFound;
        }

        command.Parameters.AddPositional(userId, NpgsqlDbType.Bigint);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return UserLoginResult.NotFound;
        }

        var storedHash = Convert.FromHexString(reader.GetString(0));
        var salt = Convert.FromHexString(reader.GetString(1));
        var hash = KeyDerivation.Pbkdf2(request.Password, salt, KeyDerivationPrf.HMACSHA256, 100_000, 64);

        if (!CryptographicOperations.FixedTimeEquals(hash, storedHash))
        {
            return UserLoginResult.InvalidPassword;
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

        return UserLoginResult.Success(tokenHandler.WriteToken(token));
    }

    public async Task<string> RegisterAsync(UserRegisterRequest request)
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

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

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

        return userId.ToString();
    }

    public async Task<UserGetResult?> GetAsync(string id)
    {
        const string sql = "SELECT first_name, second_name, biography, city, birthdate FROM account_info WHERE account_id = ($1) LIMIT 1";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        if (!long.TryParse(id, out var userId))
        {
            return null;
        }

        command.Parameters.AddPositional(userId, NpgsqlDbType.Bigint);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new UserGetResult(
            id,
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetFieldValue<DateOnly>(4));
    }

    public async Task<List<UserGetResult>> SearchAsync(string firstName, string secondName)
    {
        const string sql =
            """
            SELECT id, first_name, second_name, biography, city, birthdate
            FROM account_info
            WHERE first_name LIKE CONCAT($1, '%')
              AND second_name LIKE CONCAT($2, '%')
            ORDER BY id
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddPositional(firstName, NpgsqlDbType.Varchar);
        command.Parameters.AddPositional(secondName, NpgsqlDbType.Varchar);

        await using var reader = await command.ExecuteReaderAsync();
        var users = new List<UserGetResult>();

        while (await reader.ReadAsync())
        {
            users.Add(new UserGetResult(
                reader.GetInt64(0).ToString(),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetFieldValue<DateOnly>(5)));
        }

        return users;
    }
}