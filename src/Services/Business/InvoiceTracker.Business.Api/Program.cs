using InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;
using InvoiceTracker.Business.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBusinessInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapDefaultHealthChecks();

app.Run();
