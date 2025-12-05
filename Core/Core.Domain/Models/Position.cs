using Core.Domain.Basics;

namespace Core.Domain.Models;

public class Position : BaseEntity
{
    public override Guid Id { get; set; }
    public string Name { get; set; }
    public double Salary { get; set; }
    public byte? SortIndex { get; set; }


    public Position(string name, double salary)
    {
        this.Name = name;
        this.Salary = salary;
    }

    public void Deconstruct(out Guid id, out string name)
    {
        id = this.Id;
        name = this.Name;
    }
    public void Deconstruct(out Guid id, out string name, out double salary)
    {
        id = this.Id;
        name = this.Name;
        salary = this.Salary;
    }
}