using $safeprojectname$.Basics;
using $safeprojectname$.Enums;
using System;

namespace $safeprojectname$.Entities
{
    public class Employee : AuditableEntity
    {
        public string PrivateNumber { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Gender Gender { get; private set; }
        public DateTime BirthDate { get; private set; }
        public string[] Phones { get; private set; }
        public Address Address { get; private set; }

        public Position Position { get; set; }
        public string PictureName { get; set; }

        //public void SetPosition(Position position)
        //{
        //    this.Position = position;
        //}
    }
}
