using InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;
using InvoiceTracker.Identity.Api.Exceptions;
using InvoiceTracker.Identity.Api.Interfaces;
using InvoiceTracker.Identity.Api.Options;
using InvoiceTracker.Identity.Api.Services;
using InvoiceTracker.Identity.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<IdentityExceptionHandler>();
builder.Services.AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddOptions<PortalOptions>()
    .BindConfiguration(PortalOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// API docs in Development only: the document at /openapi/v1.json, the Scalar UI at /scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Invoice Tracker Identity API"));
}

app.MapControllers();
app.MapDefaultHealthChecks();

app.Run();
