using Core.Domain.Basics;
using Core.Domain.Enums;

namespace Core.Domain.Models;

public class Employee : AuditableEntity
{
    public override Guid Id { get; set; }
    public string PrivateNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }

    public Language Language { get; set; }
    public string[] Phones { get; set; }
    public Address? Address { get; set; }

    public Position? Position { get; set; }
    public string? PictureName { get; set; }


    public Employee(string privateNumber, string firstName, string lastName, DateTime birthDate, Gender gender)
    {
        this.PrivateNumber = privateNumber;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.BirthDate = birthDate;
        this.Gender = gender;
        this.Language = Language.None;
        this.Phones = Array.Empty<string>();
    }

    public void SetLanguages(ICollection<Language> languages)
    {
        Language language = Language.None;
        foreach (var item in languages)
            language |= item;

        this.Language = language;
    }

    public void Deconstruct(out Guid id, out string privateNumber, out string firstName, out string lastName)
    {
        id = this.Id;
        privateNumber = this.PrivateNumber;
        firstName = this.FirstName;
        lastName = this.LastName;
    }
    public void Deconstruct(out Guid id, out string privateNumber, out string firstName, out string lastName, out Gender gender, out DateTime birthDate)
    {
        id = this.Id;
        privateNumber = this.PrivateNumber;
        firstName = this.FirstName;
        lastName = this.LastName;
        gender = this.Gender;
        birthDate = this.BirthDate;
    }
}