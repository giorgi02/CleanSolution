namespace Core.Shared.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateTime birthDate)
    {
        var today = DateTime.Today;

        if (birthDate.Date > today)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));

        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;

        return age;
    }
}
