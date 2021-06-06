using CleanSolution.Core.Application.Interfaces.Repositories;
using CleanSolution.Core.Domain.Entities;

namespace $safeprojectname$.Implementations.Repositories
{
    internal class PositionRepository : Repository<Position>, IPositionRepository
    {
        public PositionRepository(DataContext context) : base(context) { }

        // todo: კეთდება C# ის ახალი ფუნქციონალით, შვილით გადატვირთვა
        public override PositionRepository Test()
        {
            return new PositionRepository(context);
        }
    }
}
