using Core.Application;
using HealthChecks.UI.Client;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Presentation.WebApi.Extensions;
using Presentation.WebApi.Extensions.Configurations;
using Presentation.WebApi.Extensions.Middlewares;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.AddThisLayer();

builder.Services.AddApplicatonLayer(builder.Configuration);
builder.Services.AddMessagingLayer(builder.Configuration);
builder.Services.AddPersistenceLayer(builder.Configuration);

var app = builder.Build();
// Configure the HTTP request pipeline.


app.UseSwaggerMiddleware();

app.UseCorrelationId();

app.UseMiddleware<ExceptionHandler>();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();