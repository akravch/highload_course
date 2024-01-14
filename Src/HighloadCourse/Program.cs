using System.Text.Json;
using HighloadCourse.ErrorHandling;
using HighloadCourse.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options => options.Filters.Add<GlobalModelValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressMapClientErrors = true;
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Configuration.GetSection("ConnectionStrings");
builder.Configuration.GetConnectionString("ReadWrite");

var readWriteConnectionString = builder.Configuration.GetConnectionString("ReadWrite")!;
var readWriteDataSource = NpgsqlDataSource.Create(readWriteConnectionString);

var readOnlyConnectionString = builder.Configuration.GetConnectionString("ReadOnly")!;
var readOnlyDataSource = NpgsqlDataSource.Create(readOnlyConnectionString);

builder.Services.AddKeyedSingleton("ReadWrite", readWriteDataSource);
builder.Services.AddKeyedSingleton("ReadOnly", readOnlyDataSource);

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserServiceReadOnly>();
builder.Services.AddTransient<GlobalModelValidationFilter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(_ => { });
app.MapControllers();
app.Run();
