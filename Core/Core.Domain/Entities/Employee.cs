using Core.Domain.Basics;
using Core.Domain.Enums;
using System.Linq.Expressions;

namespace Core.Domain.Entities;
public class Employee : AuditableEntity, IAggregateRoot
{
    public string PrivateNumber { get; private set; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateTime BirthDate { get; init; }
    public Gender Gender { get; init; }

    public Language Language { get; private set; }
    public string[] Phones { get; private set; }
    public Address? Address { get; private set; }

    public Position? Position { get; set; }
    public string? PictureName { get; set; }


    //#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    //    private Employee() { /* for deserialization "AutoMapper" */ }
    //#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Employee(string privateNumber, string firstName, string lastName, DateTime birthDate, Gender gender)
    {
        this.PrivateNumber = privateNumber;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.Gender = gender;
        this.BirthDate = birthDate;
        this.Language = Language.None;
        this.Phones = Array.Empty<string>();
    }
    public Employee(string privateNumber, string firstName, string lastName, DateTime birthDate, Gender gender,
        Language language, string[] phones, Address? address, Position? position, string? pictureName)
     : this(privateNumber, firstName, lastName, birthDate, gender)
    {
        this.Language = language;
        this.Phones = phones;
        this.Address = address;
        this.Position = position;
        this.PictureName = pictureName;
    }

    public void SetLanguages(ICollection<Language> languages)
    {
        Language language = Language.None;
        foreach (var item in languages)
            language |= item;

        this.Language = language;
    }

    public void Deconstruct(out Guid id, out string privateNumber, out string firstName, out string lastName, out Gender gender, out DateTime birthDate, out string[] phones, out Language language, out Address? address, out Position? position, out string? pictureName)
    {
        id = this.Id;
        privateNumber = this.PrivateNumber;
        firstName = this.FirstName;
        lastName = this.LastName;
        gender = this.Gender;
        birthDate = this.BirthDate;
        phones = this.Phones;
        language = this.Language;
        address = this.Address;
        position = this.Position;
        pictureName = this.PictureName;
    }


    public Expression<Func<Employee, bool>> ToFilterExpression() =>
        x => (this.Id == default || x.Id == this.Id)
        && (this.PrivateNumber == default || x.PrivateNumber == this.PrivateNumber)
        && (this.FirstName == default || x.FirstName == this.FirstName)
        && (this.LastName == default || x.LastName == this.LastName)
        && (this.BirthDate == default || x.BirthDate == this.BirthDate)
        && (this.Gender == default || x.Gender == this.Gender)
        && (this.LastName == default || x.LastName == this.LastName)
        && (this.PictureName == default || x.PictureName == this.PictureName)
        && (this.Address == default || x.Address == this.Address)
        && (this.Position != null && this.Position.ToFilterExpression().Compile().Invoke(x.Position!));

    public Expression<Func<Employee, bool>> ToSearchExpression() =>
        x => x.Id == this.Id
        || x.PrivateNumber == this.PrivateNumber
        || x.FirstName == this.FirstName
        || x.LastName == this.LastName
        || x.BirthDate == this.BirthDate
        || x.Gender == this.Gender
        || x.LastName == this.LastName
        || x.PictureName == this.PictureName
        || x.Address == this.Address
        || (this.Position != null && this.Position.ToSearchExpression().Compile().Invoke(x.Position!));
}
