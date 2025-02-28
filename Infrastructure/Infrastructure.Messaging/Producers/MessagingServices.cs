using Core.Application.Interfaces.Services;
using Core.Domain.Models;

namespace Infrastructure.Messaging.Producers;
internal class MessagingServices : IMessagingService
{
    public Task TodoItemCreated(TodoItem todoItem)
    {
        throw new NotImplementedException();
    }
    public Task TodoItemUpdated(TodoItem todoItem)
    {
        throw new NotImplementedException();
    }

    public Task TodoItemDeleted(TodoItem todoItem)
    {
        throw new NotImplementedException();
    }
}