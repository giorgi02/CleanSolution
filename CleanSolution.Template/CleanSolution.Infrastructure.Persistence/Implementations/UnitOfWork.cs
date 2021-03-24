﻿using CleanSolution.Core.Application.Interfaces;
using CleanSolution.Core.Application.Interfaces.Repositories;
using $safeprojectname$.Implementations.Repositories;

namespace $safeprojectname$.Implementations
{
    internal class UnitOfWork : IUnitOfWork
    {
        private IPositionRepository positionRepository;
        private IEmployeeRepository employeeRepository;


        private readonly DataContext context;
        public UnitOfWork(DataContext context) => this.context = context;


        public IPositionRepository PositionRepository => positionRepository ??= new PositionRepository(context);
        public IEmployeeRepository EmployeeRepository => employeeRepository ??= new EmployeeRepository(context);
    }
}
