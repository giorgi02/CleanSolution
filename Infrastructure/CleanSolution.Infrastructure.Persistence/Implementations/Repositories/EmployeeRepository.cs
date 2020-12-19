﻿using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Core.Domain.Entities;

namespace CleanSolution.Infrastructure.Persistence.Implementations.Repositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(DataContext context) : base(context)
        {

        }
    }
}
