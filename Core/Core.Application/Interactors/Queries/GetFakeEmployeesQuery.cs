using Bogus;
using Core.Application.DTOs;
using Core.Domain.Models;

namespace Core.Application.Interactors.Queries;

public abstract class GetFakeEmployeesQuery
{
    public record struct Request : IRequest<List<GetEmployeeDto>>;


    public sealed class Handler : IRequestHandler<Request, List<GetEmployeeDto>>
    {
        public Task<List<GetEmployeeDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var userFaker = new Faker<GetEmployeeDto>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.PrivateNumber, f => f.Random.ReplaceNumbers("##########"))
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Gender, f => f.PickRandom(new[] { "Male", "Female" }))
                .RuleFor(u => u.Age, f => f.Random.Int(18, 65))
                .RuleFor(u => u.Phones, f => [f.Phone.PhoneNumber()])
                .RuleFor(u => u.Address, f => new Address(f.Address.City(), f.Address.StreetAddress()))
                .RuleFor(u => u.Position, f => new GetPositionDto
                {
                    Id = Guid.NewGuid(),
                    Name = f.Name.JobTitle(),
                    Salary = f.Random.Decimal(1000, 10000)
                });

            var fakeUsers = userFaker.Generate(10);

            return Task.FromResult(fakeUsers);
        }
    }
}
