using Core.Application.Commons;
using Core.Domain.Models;

namespace Core.Application.Interfaces.Repositories;
public interface ITodoItemRepository
{
    Task<Pagination<TodoItem>> SearchAsync(int pageIndex, int pageSize, string? text);
    Task<TodoItem> GetAsync(Guid id);
    Task<TodoItem> AddAsync(TodoItem todoItem);
}