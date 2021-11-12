﻿using $safeprojectname$.Commons;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace $safeprojectname$.Interfaces.Repositories
{
    public interface IEmployeeRepository : IRepository<Guid, Employee>
    {
        Task<Pagination<Employee>> FilterAsync(int pageIndex, int pageSize, string privateNumber = null, string firatName = null, string lastName = null, Gender? gender = null);
        Task<Pagination<Employee>> SearchAsync(int pageIndex, int pageSize, string text);
    }
}