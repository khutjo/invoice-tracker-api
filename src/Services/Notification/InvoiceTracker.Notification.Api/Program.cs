using InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;
using InvoiceTracker.Notification.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNotificationInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapDefaultHealthChecks();

app.Run();
