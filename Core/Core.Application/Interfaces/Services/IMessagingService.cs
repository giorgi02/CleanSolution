using Core.Domain.Models;

namespace Core.Application.Interfaces.Services;
public interface IMessagingService
{
    Task TodoItemCreated(TodoItem todoItem);
    Task TodoItemUpdated(TodoItem todoItem);
    Task TodoItemDeleted(TodoItem todoItem);
}
