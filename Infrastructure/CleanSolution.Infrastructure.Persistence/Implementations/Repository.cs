using CleanSolution.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanSolution.Infrastructure.Persistence.Implementations
{
    public class Repository<IEntity> : IRepository<IEntity> where IEntity : class
    {
        private readonly DataContext context;

        public Repository(DataContext context)
        {
            this.context = context;
        }
    }
}
