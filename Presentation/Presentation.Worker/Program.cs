using Core.Application.Interactors.Notifications;
using Core.Shared;
using Presentation.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSlimTaskScheduler();

builder.Services.AddTransient<UpsertPositionNotification.Handler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
