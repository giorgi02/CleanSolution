using CleanSolution.Core.Domain.Basics;

namespace CleanSolution.Core.Domain.Entities
{
    public class Position : BaseEntity
    {
        public string Name { get; private set; }
        public double Salary { get; private set; }
    }
}
