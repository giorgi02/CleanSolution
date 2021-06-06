using AutoMapper;
using $safeprojectname$.DTOs;
using $safeprojectname$.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace $safeprojectname$.Features.Positions.Queries
{
    public class GetPositionQuery
    {
        public class Request : IRequest<IEnumerable<GetPositionDto>> { }

        public class Handler : IRequestHandler<Request, IEnumerable<GetPositionDto>>
        {
            private readonly IUnitOfWork unit;
            private readonly IMapper mapper;

            public Handler(IUnitOfWork unit, IMapper mapper) => (this.unit, this.mapper) = (unit, mapper);

            public async Task<IEnumerable<GetPositionDto>> Handle(Request request, CancellationToken cancellationToken)
            {
                var positions = await unit.PositionRepository.ReadAsync();

                return mapper.Map<IEnumerable<GetPositionDto>>(positions);
            }
        }
    }
}
