using AspNetCoreRateLimit;
using Core.Application;
using Infrastructure.Documents;
using Infrastructure.Logger;
using Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using Presentation.WebApi.Extensions;
using Presentation.WebApi.Extensions.Configurations;
using Presentation.WebApi.Extensions.Middlewares;

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
