using AspNetCoreRateLimit;
using Core.Application;
using HealthChecks.UI.Client;
// @if documents
using Infrastructure.Documents;
// @endif
// @if messaging
using Infrastructure.Messaging;
// @endif
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Presentation.WebApi.Extensions;
using Presentation.WebApi.Extensions.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.AddStartup();

builder.Services.AddApplicationLayer(builder.Configuration);

// @if documents
builder.Services.AddDocumentsLayer(builder.Configuration);
// @endif
// @if messaging
builder.Services.AddMessagingLayer(builder.Configuration);
builder.Services.AddMessagingConsumers(); // this host runs the Kafka consumers
// @endif
builder.Services.AddPersistenceLayer(builder.Configuration);

var app = builder.Build();
// Configure the HTTP request pipeline. The order matters.

if (app.Environment.IsDevelopment())
{
    // AllowAnonymous: the API docs stay reachable when the JWT fallback policy is enabled
    app.MapOpenApi().AllowAnonymous();

    app.MapScalarApiReference().AllowAnonymous(); // UI at /scalar/v1
    app.UseCustomSwaggerUI();                     // UI at /swagger/index.html
}

app.UseCorrelationId();

app.UseResponseCompression();

app.UseMiddleware<ExceptionHandler>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIpRateLimiting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

app.Run();
