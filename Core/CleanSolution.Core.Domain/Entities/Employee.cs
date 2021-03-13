﻿using CleanSolution.Core.Domain.Basics;
using CleanSolution.Core.Domain.Enums;
using System;

namespace CleanSolution.Core.Domain.Entities
{
    public class Employee : AuditableEntity
    {
        public string PrivateNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public Address Address { get; set; }
        public Position Position { get; set; }
    }
}
