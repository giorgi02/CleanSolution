﻿using Core.Application.DTOs;
using System.Diagnostics;

namespace Core.Application.Interactors.Notifications;
public abstract class CheckoutProcessCommand
{
    public record class Request(QueueItemDto item) : INotification;

    public sealed class Handler : INotificationHandler<Request>
    {
        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            Debug.WriteLine($"send email {request.item.OrderId}");
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}