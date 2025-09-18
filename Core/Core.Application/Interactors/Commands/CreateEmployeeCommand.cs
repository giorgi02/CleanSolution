using Core.Application.DTOs;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Enums;
using Core.Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Interactors.Commands;
public abstract class CreateEmployeeCommand
{
    public sealed record class Request : IRequest<GetEmployeeDto>
    {
        public IFormFile? Picture { get; set; }
        //[Required]
        //[StringLength(maximumLength: 11, MinimumLength = 11, ErrorMessage = "პირადი ნომერი უნდა შედგებოდეს 11 სიმბოლოსგან")]
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
            Languages = new HashSet<Language>();
            Phones = Array.Empty<string>();
        }
    }


    public sealed class Handler : IRequestHandler<Request, GetEmployeeDto>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IDocumentService _documents;
        private readonly IMessagingService _messaging;

        public Handler(IServiceProvider services)
        {
            _employeeRepository = services.GetRequiredService<IEmployeeRepository>();
            _positionRepository = services.GetRequiredService<IPositionRepository>();
            _documents = services.GetRequiredService<IDocumentService>();
            _messaging = services.GetRequiredService<IMessagingService>();
        }

        public async Task<GetEmployeeDto> Handle(Request request, CancellationToken cancellationToken)
        {
            var position = await _positionRepository.ReadAsync(request.PositionId, cancellationToken);
            _ = position ?? throw new EntityNotFoundException("ზე ჩანაწერი ვერ მოიძებნა", nameof(request.PositionId));

            cancellationToken.ThrowIfCancellationRequested();

            var employee = new Employee(request.PrivateNumber!, request.FirstName!, request.LastName!, request.BirthDate, request.Gender);
            employee.SetLanguages(request.Languages);
            employee.Position = position;

            if (request.Picture != null)
                employee.PictureName = await _documents.SaveAsync(request.Picture.OpenReadStream(), request.Picture.FileName);

            cancellationToken.ThrowIfCancellationRequested();

            var result = await _employeeRepository.CreateAsync(employee, cancellationToken);
            await _messaging.EmployeeCreated(employee);

            return result.Adapt<GetEmployeeDto>();
        }
    }


    public sealed class Validator : AbstractValidator<Request>
    {
        private readonly IPositionRepository _positionRepository;
        private readonly TimeProvider _timeProvider;

        public Validator(IPositionRepository positionRepository, TimeProvider timeProvider)
        {
            _positionRepository = positionRepository;
            _timeProvider = timeProvider;

            RuleFor(x => x.PrivateNumber)
                .NotNull().WithMessage("პირადი ნომერი ცარიელია")
                .Length(11).WithMessage("პირადი ნომერი უნდა შედგებოდეს 11 სიმბოლოსგან")
                .Matches("^[0-9]*$").WithMessage("პირადი ნომერი უნდა შედგებოდეს მხოლოდ ციფრებისგან");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("სახელის ველი ცარიელია");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("გვარის ველი ცარიელია");

            RuleFor(x => x.Gender).IsInEnum().WithMessage("მიუთითეთ სქესი სწორად");

            RuleFor(x => x.BirthDate)
                .Must(y => y < _timeProvider.GetLocalNow()).WithMessage("მიუთითეთ დაბადების თარიღი სწორად")
                .Must(y => y < _timeProvider.GetLocalNow().AddYears(-18) || y > _timeProvider.GetLocalNow().AddYears(-100)).WithMessage("ამ ასაკის პიროვნება არ შეიძლება იყოს დასაქმებული");

            //RuleFor(x => x.PositionId)
            //    .MustAsync(IfExistPosition).WithMessage("მიუთითეთ პოზიცია სწორად");
        }

        private async Task<bool> IfExistPosition(Guid positionId, CancellationToken cancellationToken)
        {
            return await _positionRepository.CheckAsync(x => x.Id == positionId);
        }
    }
}