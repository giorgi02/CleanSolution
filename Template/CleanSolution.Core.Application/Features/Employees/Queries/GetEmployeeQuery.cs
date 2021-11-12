﻿using AutoMapper;
using $safeprojectname$.DTOs;
using $safeprojectname$.Exceptions;
using $safeprojectname$.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace $safeprojectname$.Features.Employees.Queries
{
    public class GetEmployeeQuery
    {
        public class Request : IRequest<GetEmployeeDto>
        {
            public Guid EmploueeId { get; private set; }

            public Request(Guid employeeId) => this.EmploueeId = employeeId;
        }

        public class Handler : IRequestHandler<Request, GetEmployeeDto>
        {
            private readonly IUnitOfWork unit;
            private readonly IMapper mapper;

            public Handler(IUnitOfWork unit, IMapper mapper)
            {
                this.unit = unit;
                this.mapper = mapper;
            }

            public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
            {
                var application = await unit.EmployeeRepository.ReadAsync(request.EmploueeId);
                if (application == null)
                    throw new EntityNotFoundException("ჩანაწერი ვერ მოიძებნა");

                return await Task.FromResult(mapper.Map<GetEmployeeDto>(application));
            }
        }
    }
}