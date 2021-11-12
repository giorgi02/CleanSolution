﻿using AutoMapper;
using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanSolution.Core.Application.Features.Positions.Queries
{
    public class GetPositionQuery
    {
        public record Request() : IRequest<IEnumerable<GetPositionDto>>;


        public class Handler : IRequestHandler<Request, IEnumerable<GetPositionDto>>
        {
            private readonly IUnitOfWork _unit;
            private readonly IMapper _mapper;

            public Handler(IUnitOfWork unit, IMapper mapper) =>
                (_unit, _mapper) = (unit, mapper);

            public async Task<IEnumerable<GetPositionDto>> Handle(Request request, CancellationToken cancellationToken)
            {
                var positions = await _unit.PositionRepository.ReadAsync();

                return _mapper.Map<IEnumerable<GetPositionDto>>(positions);
            }
        }
    }
}
