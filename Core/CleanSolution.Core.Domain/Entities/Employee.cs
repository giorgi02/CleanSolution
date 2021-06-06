using CleanSolution.Core.Domain.Basics;
using CleanSolution.Core.Domain.Enums;
using System;

namespace CleanSolution.Core.Domain.Entities
{
    public class Employee : AuditableEntity
    {
        public string PictureName { get; private set; }
        public string PrivateNumber { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Gender Gender { get; private set; }
        public DateTime BirthDate { get; private set; }
        public string[] Phones { get; private set; }
        public Address Address { get; private set; }
        public Position Position { get; private set; }

        //public void SetPosition(Position position)
        //{
        //    this.Position = position;
        //}
    }
}
