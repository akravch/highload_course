using HighloadCourse.Models;
using Npgsql;
using NpgsqlTypes;

namespace HighloadCourse.Services;

public sealed class UserService : IDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public UserService(IConfiguration configuration)
    {
        _dataSource = NpgsqlDataSource.Create(configuration["ConnectionString"]!);
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }

    public async Task<UserLoginResponse> LoginAsync(UserLoginRequest request)
    {
        return new UserLoginResponse { Token = "token" };

    }

    public async Task<UserRegisterResponse> RegisterAsync(UserRegisterRequest request)
    {
        return new UserRegisterResponse { UserId = "id" };

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