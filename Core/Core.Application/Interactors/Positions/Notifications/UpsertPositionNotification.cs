namespace Core.Application.Interactors.Positions.Notifications;
public abstract class UpsertPositionNotification
{
    public sealed record class Request : INotification
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double Salary { get; set; }
        public byte? SortIndex { get; set; }
    }

    public sealed class Handler : INotificationHandler<Request>
    {
        public async Task Handle(Request request, CancellationToken cancellationToken)
        {
            await Task.Delay(500);
        }
    }
}