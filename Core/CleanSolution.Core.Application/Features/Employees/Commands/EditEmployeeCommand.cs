﻿using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;

namespace CleanSolution.Core.Application.Features.Employees.Commands;
public class EditEmployeeCommand
{
    public class Request : IRequest
    {
        public Guid EmployeeId { get; private set; }
        public int Version { get; set; }

        public string? PrivateNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Gender Gender { get; set; }
        public ICollection<Language> Languages { get; set; }
        public DateTime BirthDate { get; set; }
        public string[] Phones { get; set; }
        public Address? Address { get; set; }
        public Guid PositionId { get; set; }


        public Request()
        {
            this.Languages = new HashSet<Language>();
            this.Phones = Array.Empty<string>();
        }
        public void SetId(Guid employeeId) => this.EmployeeId = employeeId;
    }


    public class Handler : IRequestHandler<Request>
    {
        public Handler()
        {
        }

        public Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Unit.Value);
        }
    }
}