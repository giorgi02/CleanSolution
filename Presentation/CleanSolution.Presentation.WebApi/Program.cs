using AspNetCoreRateLimit;
using CleanSolution.Core.Application;
using CleanSolution.Infrastructure.Documents;
using CleanSolution.Infrastructure.Logger;
using CleanSolution.Infrastructure.Persistence;
using CleanSolution.Presentation.WebApi.Extensions;
using CleanSolution.Presentation.WebApi.Extensions.Configurations;
using CleanSolution.Presentation.WebApi.Extensions.Middlewares;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicatonLayer(builder.Configuration);

builder.Services.AddDocumentsLayer(builder.Configuration);
builder.Host.AddLoggerLayer(builder.Configuration);
builder.Services.AddPersistenceLayer(builder.Configuration);

builder.Services.AddThisLayer(builder.Configuration);


var app = builder.Build();


app.UseRequestLocalization(app.Services.GetService<IOptions<RequestLocalizationOptions>>()!.Value);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerMiddleware();
}

// todo: დავაკვირდე ამ middleware-ს
app.UseSerilogRequestLogging();

app.UseRequestLocalization();

app.UseMiddleware<ExceptionHandler>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIpRateLimiting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapCustomHealthCheck();

app.MapControllers();

app.Run();
