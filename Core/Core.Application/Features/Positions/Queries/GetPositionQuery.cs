using Core.Application.DTOs;
using Core.Application.Interfaces.Repositories;

namespace Core.Application.Features.Positions.Queries;
public sealed class GetPositionQuery
{
    public record struct Request : IRequest<IEnumerable<GetPositionDto>>;


    public sealed class Handler : IRequestHandler<Request, IEnumerable<GetPositionDto>>
    {
        private readonly IPositionRepository _repository;
        private readonly IMapper _mapper;

        public Handler(IPositionRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<GetPositionDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var positions = await _repository.ReadAsync();

            return _mapper.Map<IEnumerable<GetPositionDto>>(positions);
        }
    }
}