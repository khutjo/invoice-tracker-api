using InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;
using InvoiceTracker.Invoicing.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInvoicingInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapDefaultHealthChecks();

app.Run();
