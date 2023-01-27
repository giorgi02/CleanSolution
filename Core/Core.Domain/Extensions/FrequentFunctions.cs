namespace Core.Domain.Extensions;
public static class FrequentFunctions
{
    public static int CalculateAge(DateTime birthDate)
    {
        int age = DateTime.Now.Year - birthDate.Year;
        if (DateTime.Now < birthDate.AddYears(age))
            age--;

        return age;
    }
}