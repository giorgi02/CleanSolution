using CleanSolution.Core.Domain.Basics;

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
    }
}
