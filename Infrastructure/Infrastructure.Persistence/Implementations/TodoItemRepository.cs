using Core.Application.Commons;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Models;

namespace Infrastructure.Persistence.Implementations;
internal sealed class TodoItemRepository(DataContext context) : ITodoItemRepository
{
    public Task<TodoItem> AddAsync(TodoItem todoItem)
    {
        throw new NotImplementedException();
    }

    public Task<TodoItem> GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Pagination<TodoItem>> SearchAsync(int pageIndex, int pageSize, string text)
    {
        throw new NotImplementedException();
    }
}