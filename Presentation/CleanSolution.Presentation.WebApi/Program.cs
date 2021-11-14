using CleanSolution.Core.Application;
using CleanSolution.Infrastructure.Files;
using CleanSolution.Infrastructure.Persistence;
using CleanSolution.Presentation.WebApi.Extensions;
using CleanSolution.Presentation.WebApi.Extensions.Middlewares;
using Serilog;
using Workabroad.Infrastructure.Logger;
using Workabroad.Presentation.WebApi.Extensions.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicatonLayer(builder.Configuration);

builder.Services.AddFilesLayer(builder.Configuration);
builder.Host.AddLoggerLayer(builder.Configuration);
builder.Services.AddPersistenceLayer(builder.Configuration);

builder.Services.AddThisLayer(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwaggerMiddleware("CleanSolution v1");
}

// todo: დავაკვირდე ამ middleware-ს
app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandler>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapCustomHealthCheck();

app.MapControllers();

app.Run();
