namespace Core.Application.DTOs;

public class QueueItemDto
{
    public Guid OrderId { get; set; }
    public string? Text { get; set; }
}
