namespace Core.Shared.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateTime birthDate) => birthDate.CalculateAge(DateTime.Today);

    public static int CalculateAge(this DateTime birthDate, DateTime today)
    {
        if (birthDate.Date > today.Date)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));

        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.Date.AddYears(-age)) age--;

        return age;
    }
}
