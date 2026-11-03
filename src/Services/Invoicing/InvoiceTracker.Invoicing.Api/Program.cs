using System.Text.Json.Serialization;
using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using InvoiceTracker.BuildingBlocks.ServiceDefaults.Database;
using InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;
using InvoiceTracker.Invoicing.Api.Exceptions;
using InvoiceTracker.Invoicing.Api.Interfaces;
using InvoiceTracker.Invoicing.Api.Jobs;
using InvoiceTracker.Invoicing.Api.OpenApi;
using InvoiceTracker.Invoicing.Api.Services;
using InvoiceTracker.Invoicing.Api.Webhooks;
using InvoiceTracker.Invoicing.Infrastructure;
using InvoiceTracker.Invoicing.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Enums travel as their names ("Draft", "Sent"), not numbers
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<InvoiceExceptionHandler>();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddInvoicingInfrastructure(builder.Configuration);

builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddOptions<OverdueScanOptions>()
    .BindConfiguration(OverdueScanOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddScoped<IOverdueScanService, OverdueScanService>();
builder.Services.AddHostedService<OverdueScanner>();

builder.Services.AddOptions<PaymentWebhookOptions>()
    .BindConfiguration(PaymentWebhookOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();

var app = builder.Build();

await app.MigrateDbContextAsync<InvoicingDbContext>();

app.UseExceptionHandler();
app.UseStatusCodePages();

// API docs in Development only: the document at /openapi/v1.json, the Scalar UI at /scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("Invoice Tracker Invoicing API")
        .AddPreferredSecuritySchemes(BearerSecuritySchemeTransformer.SchemeName));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultHealthChecks();

await app.RunAsync();
