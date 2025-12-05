using Core.Application.DTOs;
using Core.Application.Interfaces.Repositories;
using Mapster;

namespace Core.Application.Interactors.Queries;

public abstract class GetPositionsQuery
{
    public record struct Request : IRequest<IEnumerable<GetPositionDto>>;


    public sealed class Handler : IRequestHandler<Request, IEnumerable<GetPositionDto>>
    {
        private readonly IPositionRepository _repository;
        public Handler(IPositionRepository repository) =>
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));


        public async Task<IEnumerable<GetPositionDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var positions = await _repository.ReadAsync();

            return positions.Adapt<IEnumerable<GetPositionDto>>();
        }
    }
}