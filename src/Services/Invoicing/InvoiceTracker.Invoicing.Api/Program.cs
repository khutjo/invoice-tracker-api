using InvoiceTracker.Invoicing.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInvoicingInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
