using HighloadCourse.Models;
using HighloadCourse.Utils;
using Npgsql;
using NpgsqlTypes;

namespace HighloadCourse.Services;

public class UserServiceReadOnly
{
    private readonly NpgsqlDataSource _dataSource;

    public UserServiceReadOnly([FromKeyedServices("ReadOnly")] NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
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