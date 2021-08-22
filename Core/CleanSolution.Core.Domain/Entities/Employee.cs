using CleanSolution.Core.Domain.Basics;
using CleanSolution.Core.Domain.Enums;
using System;

namespace CleanSolution.Core.Domain.Entities
{
    public class Employee : AuditableEntity, IAggregateRoot
    {
        public string PrivateNumber { get; init; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public DateTime BirthDate { get; private set; }
        public Gender Gender { get; private set; }
        public Language Language { get; private set; }
        public string[] Phones { get; private set; }
        public Address Address { get; private set; }

        public Position Position { get; set; }
        public string PictureName { get; set; }


        private Employee() { /* for deserialization & ORMs */}
        public Employee(string privateNumber, string firstName, string lastName, Gender gender, DateTime birthDate, string[] phones, Language language, Address address, Position position, string pictureName)
            : this()
        {
            this.PrivateNumber = privateNumber;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Gender = gender;
            this.BirthDate = birthDate;
            this.Phones = phones;
            this.Language = language;
            this.Address = address;
            this.Position = position;
            this.PictureName = pictureName;
        }

        public void Deconstruct(out Guid id, out string privateNumber, out string firstName, out string lastName, out Gender gender, out DateTime birthDate, out string[] phones, out Language language, out Address address, out Position position, out string pictureName)
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
    }
}
