namespace Core.Domain.Shared;
public interface IDateTimeProvider
{
    public DateTime Now { get; }
}
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}