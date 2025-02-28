namespace Core.Application.DTOs;
public class GetTodoItemDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Note { get; set; }
    public int Count { get; set; }
}