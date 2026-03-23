using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.RegularExpressions;
using TestFirstProject;
using TestFirstProject.Contexts;
using TestFirstProject.Endpoints;
using TestFirstProject.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();
builder.ConfigureSentry();

builder.Services.AddLoggingInfrastructure(builder.Configuration);
builder.Services.AddTelemetryServices(builder.Configuration);
builder.Services.AddAuditLogging(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddDbContext<PersonsContext>();
builder.Services.AddScoped<OperationsRepository>();

var app = builder.Build();

app.UseLoggingMiddleware();
app.UseErrorTracking();
app.UseAuditAccessControl();

app.UseStaticFiles();

app.MapAuditEndpoints();
app.MapLogEndpoints();

app.MapGet("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.GetPersons(request, response);
});
app.MapPost("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.CreatePerson(response, request);
});
app.MapPut("/api/users", async (OperationsRepository operations, HttpResponse response, HttpRequest request) =>
{
    await operations.UpdatePerson(response, request);
});
app.MapDelete("/api/users/{id}", async (string id, OperationsRepository operations, HttpResponse response) =>
{
    var regex = new Regex(@"^[A-Fa-f0-9]{3}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}$");
    if (!regex.IsMatch(id))
    {
        response.StatusCode = 400;
        await response.WriteAsync("Invalid ID format. Expected format: xxx-xxxx-xxxx");
        return;
    }

    await operations.DeletePerson(response, id);
});
app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();

public partial class Program { }
