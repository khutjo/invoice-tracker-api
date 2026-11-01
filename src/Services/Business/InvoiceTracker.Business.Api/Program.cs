using System.Text.Json.Serialization;
using InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;
using InvoiceTracker.BuildingBlocks.Messaging;
using InvoiceTracker.BuildingBlocks.Messaging.RabbitMq;
using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;
using InvoiceTracker.Business.Api.Consumers;
using InvoiceTracker.Business.Api.Exceptions;
using InvoiceTracker.Business.Api.Interfaces;
using InvoiceTracker.Business.Api.OpenApi;
using InvoiceTracker.Business.Api.Services;
using InvoiceTracker.Business.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Enums travel as their names ("Cheque", "Mobile"), not numbers
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BusinessExceptionHandler>();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddBusinessInfrastructure(builder.Configuration);

builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<ReferenceDataValidator>();
builder.Services.AddScoped<ILookupService, LookupService>();

builder.Services.AddMessaging();
builder.Services.AddEventConsumer(RabbitMqTopology.BusinessQueue)
    .AddHandler<BusinessRegistered, BusinessRegisteredHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// API docs in Development only: the document at /openapi/v1.json, the Scalar UI at /scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("Invoice Tracker Business API")
        .AddPreferredSecuritySchemes(BearerSecuritySchemeTransformer.SchemeName));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultHealthChecks();

app.Run();
