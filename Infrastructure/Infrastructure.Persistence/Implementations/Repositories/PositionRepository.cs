using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;

namespace Infrastructure.Persistence.Implementations.Repositories;
internal sealed class PositionRepository : Repository<Position>, IPositionRepository
{
    public PositionRepository(DataContext context) : base(context) { }

    // todo: კეთდება C# ის ახალი ფუნქციონალით, შვილით გადატვირთვა
    public override PositionRepository Test()
    {
        return new PositionRepository(_context);
    }
}