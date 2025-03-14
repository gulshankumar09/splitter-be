using ExpenseService.API.Extensions;
using ExpenseService.API.Services;
using ExpenseService.Application.Interfaces;
using ExpenseService.Application.Services;
using ExpenseService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add after builder.Services.AddControllers();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService.Application.Services.ExpenseService>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();
builder.Services.AddScoped<IBalanceService, BalanceService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IRecurringExpenseRepository, RecurringExpenseRepository>();
builder.Services.AddScoped<IRecurringExpenseService, RecurringExpenseService>();
builder.Services.AddHostedService<RecurringExpenseBackgroundService>();

// Budget Management Services
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddHostedService<BudgetBackgroundService>();

// Banking Integration Services
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddScoped<IBankingIntegrationService, BankingIntegrationService>();
builder.Services.AddHostedService<BankingImportBackgroundService>();
builder.Services.Configure<BankingApiOptions>(builder.Configuration.GetSection("BankingApi"));
builder.Services.Configure<BankingImportOptions>(builder.Configuration.GetSection("BankingImport"));

// Calendar Integration Services
// TODO: Uncomment once calendar integration classes are implemented
//builder.Services.AddScoped<ICalendarRepository, CalendarRepository>();
//builder.Services.AddScoped<ICalendarIntegrationService, CalendarIntegrationService>();
//builder.Services.AddHostedService<CalendarSyncBackgroundService>();
//builder.Services.Configure<CalendarOptions>(builder.Configuration.GetSection("Calendar"));

// HTTP Clients for external APIs
builder.Services.AddHttpClient("BankingApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BankingApi:BaseUrl"] ?? "https://api.banking.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// TODO: Uncomment once calendar integration is implemented
//builder.Services.AddHttpClient("GoogleCalendar", client =>
//{
//    client.BaseAddress = new Uri("https://www.googleapis.com/calendar/v3/");
//    client.DefaultRequestHeaders.Add("Accept", "application/json");
//});

//builder.Services.AddHttpClient("MicrosoftGraph", client =>
//{
//    client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
//    client.DefaultRequestHeaders.Add("Accept", "application/json");
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
