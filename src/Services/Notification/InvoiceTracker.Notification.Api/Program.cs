using System.Text.Json.Serialization;
using InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;
using InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels;
using InvoiceTracker.BuildingBlocks.Messaging;
using InvoiceTracker.BuildingBlocks.Messaging.RabbitMq;
using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using InvoiceTracker.BuildingBlocks.ServiceDefaults.Database;
using InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;
using InvoiceTracker.Notification.Api.Consumers;
using InvoiceTracker.Notification.Api.Dispatching;
using InvoiceTracker.Notification.Api.Hubs;
using InvoiceTracker.Notification.Api.Interfaces;
using InvoiceTracker.Notification.Api.OpenApi;
using InvoiceTracker.Notification.Api.Options;
using InvoiceTracker.Notification.Api.Services;
using InvoiceTracker.Notification.Infrastructure;
using InvoiceTracker.Notification.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// Enums travel as their names ("InvoiceSent", "Email") in requests and responses
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddProblemDetails();
builder.Services.AddScoped<INotificationLogService, NotificationLogService>();

// Enums travel as their names ("InvoiceSent", "Email"), matching the REST API
builder.Services.AddSignalR()
    .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOptions<PortalOptions>()
    .BindConfiguration(PortalOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddMessaging();
builder.Services.AddEventConsumer(RabbitMqTopology.NotificationQueue)
    .AddHandler<EmailVerificationRequested, EmailVerificationRequestedHandler>()
    .AddHandler<InvoiceSent, InvoiceSentHandler>()
    .AddHandler<InvoiceOverdue, InvoiceOverdueHandler>()
    .AddHandler<PaymentRecorded, PaymentRecordedHandler>();

builder.Services.AddHostedService<NotificationDispatcher>();

var app = builder.Build();

await app.MigrateDbContextAsync<NotificationDbContext>();

app.UseExceptionHandler();
app.UseStatusCodePages();

// API docs in Development only: the document at /openapi/v1.json, the Scalar UI at /scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("Invoice Tracker Notification API")
        .AddPreferredSecuritySchemes(BearerSecuritySchemeTransformer.SchemeName));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>(NotificationsHub.Path);
app.MapDefaultHealthChecks();

await app.RunAsync();
