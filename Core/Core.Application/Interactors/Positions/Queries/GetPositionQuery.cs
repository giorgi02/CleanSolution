using Core.Application.DTOs;
using Core.Application.Interfaces.Repositories;
using Mapster;

namespace Core.Application.Interactors.Positions.Queries;
public abstract class GetPositionQuery
{
    public record struct Request(Guid Id) : IRequest<GetPositionDto?>;


    public sealed class Handler : IRequestHandler<Request, GetPositionDto?>
    {
        private readonly IPositionRepository _repository;
        public Handler(IPositionRepository repository) =>
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));


        public async Task<GetPositionDto?> Handle(Request request, CancellationToken cancellationToken)
        {
            var position = await _repository.ReadAsync(request.Id, cancellationToken);

            return position?.Adapt<GetPositionDto>();
        }
    }
}