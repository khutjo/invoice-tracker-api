using InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultHealthChecks();
app.MapReverseProxy();

// Only the routes in ReverseProxy:Routes are exposed; any other path gets a ProblemDetails 404
app.MapFallback(() => Results.Problem(
    title: "No route matches this path.",
    statusCode: StatusCodes.Status404NotFound));

app.Run();
