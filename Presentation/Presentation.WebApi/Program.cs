using AspNetCoreRateLimit;
using Core.Application;
using Infrastructure.Documents;
using Infrastructure.Messaging;
using Infrastructure.Persistence;
using Presentation.WebApi.Extensions;
using Presentation.WebApi.Extensions.Configurations;
using Presentation.WebApi.Extensions.Middlewares;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddApplicatonLayer(builder.Configuration);

builder.Services.AddDocumentsLayer(builder.Configuration);
builder.Services.AddMessagingLayer();
builder.Services.AddPersistenceLayer(builder.Configuration);

builder.AddThisLayer();

var app = builder.Build();
// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerMiddleware();
}

//app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandler>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization();

app.UseIpRateLimiting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapCustomHealthCheck();
app.MapControllers();

app.Run();