using CleanSolution.Core.Domain.Basics;
using System;

namespace CleanSolution.Core.Domain.Entities
{
    public class Position : BaseEntity
    {
        public string Name { get; init; }
        public double Salary { get; init; }

        private Position() { /* for deserialization & ORMs */}
        public Position(string name, double salary)
            : this()
        {
            this.Name = name;
            this.Salary = salary;
        }

        public void Deconstruct(out Guid id, out string name, out double salary)
        {
            id = this.Id;
            name = this.Name;
            salary = this.Salary;
        }
    }
}
