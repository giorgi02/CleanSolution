using Core.Application;
using Core.Application.Commons;
using Core.Application.Interfaces.Services;
using Core.Shared;
// @if documents
using Infrastructure.Documents;
// @endif
// @if messaging
using Infrastructure.Messaging;
// @endif
using Infrastructure.Persistence;
using Presentation.Worker;

var builder = Host.CreateApplicationBuilder(args);

// no HTTP request here: audit columns are written as "system"
builder.Services.AddScoped<IActiveUserService, SystemActiveUserService>();

// a complete composition root: every layer whose ports the use cases need
// (in Development the container validates every registered handler at startup)
builder.Services.AddApplicationLayer(builder.Configuration);
// @if documents
builder.Services.AddDocumentsLayer(builder.Configuration);
// @endif
// @if messaging
builder.Services.AddMessagingLayer(builder.Configuration);
// @endif
builder.Services.AddPersistenceLayer(builder.Configuration);

builder.Services.AddSlimTaskScheduler();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
