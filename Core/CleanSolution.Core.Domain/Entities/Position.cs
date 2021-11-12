﻿using CleanSolution.Core.Domain.Basics;
using System;
using System.Linq.Expressions;

namespace CleanSolution.Core.Domain.Entities
{
    public class Position : BaseEntity
    {
        public string Name { get; private init; }
        public double Salary { get; private init; }

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

        public Expression<Func<Position, bool>> ToFilterExpression() =>
            x => (this.Id == default || x.Id == this.Id)
            && (this.Name == default || x.Name == this.Name)
            && (this.Salary == default || x.Salary == this.Salary);

        public Expression<Func<Position, bool>> ToSearchExpression() =>
            x => x.Id == this.Id
            || x.Name == this.Name
            || x.Salary == this.Salary;
    }
}
