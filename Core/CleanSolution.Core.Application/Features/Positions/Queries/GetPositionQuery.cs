﻿using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Interfaces.Repositories;

namespace CleanSolution.Core.Application.Features.Positions.Queries;
public sealed class GetPositionQuery
{
    public record class Request() : IRequest<IEnumerable<GetPositionDto>>;


    public class Handler : IRequestHandler<Request, IEnumerable<GetPositionDto>>
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