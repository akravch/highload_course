using System.Text.Json;
using HighloadCourse.ErrorHandling;
using HighloadCourse.Services;

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

builder.Services.AddScoped<UserService>();
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
