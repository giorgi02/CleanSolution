using Core.Domain.Basics;

namespace Core.Domain.Models;
public class TodoItem : AuditableEntity
{
    public override Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Note { get; set; }
    public int Count { get; set; }
}
