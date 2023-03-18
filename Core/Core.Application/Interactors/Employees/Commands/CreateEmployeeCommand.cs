using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Enums;
using Core.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Interactors.Employees.Commands;
public abstract class CreateEmployeeCommand
{
    public sealed record class Request : IRequest<Guid>
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
            this.Languages = new HashSet<Language>();
            this.Phones = Array.Empty<string>();
        }
    }


    public sealed class Handler : IRequestHandler<Request, Guid>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IDocumentService _documentService;
        private readonly IMessagingService _messaging;

        public Handler(IServiceProvider services)
        {
            _employeeRepository = services.GetRequiredService<IEmployeeRepository>();
            _positionRepository = services.GetRequiredService<IPositionRepository>();
            _documentService = services.GetRequiredService<IDocumentService>();
            _messaging = services.GetRequiredService<IMessagingService>();
        }

        public async Task<Guid> Handle(Request request, CancellationToken cancellationToken)
        {
            var employee = new Employee(request.PrivateNumber!, request.FirstName!, request.LastName!, request.BirthDate, request.Gender);
            employee.SetLanguages(request.Languages);
            employee.Position = await _positionRepository.ReadAsync(request.PositionId);

            cancellationToken.ThrowIfCancellationRequested();

            if (request.Picture != null)
                employee.PictureName = await _documentService.SaveAsync(request.Picture.FileName, "", request.Picture.OpenReadStream());

            cancellationToken.ThrowIfCancellationRequested();

            await _employeeRepository.CreateAsync(employee, cancellationToken);
            await _messaging.EmployeeCreated(employee);

            return employee.Id;
        }
    }


    public sealed class Validator : AbstractValidator<Request>
    {
        private readonly IPositionRepository _positionRepository;

        public Validator(IPositionRepository positionRepository)
        {
            _positionRepository = positionRepository;

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
                .Must(y => y < DateTime.Now).WithMessage("მიუთითეთ დაბადების თარიღი სწორად")
                .Must(y => y < DateTime.Now.AddYears(-18) || y > DateTime.Now.AddYears(-100)).WithMessage("ამ ასაკის პიროვნება არ შეიძლება იყოს დასაქმებული");

            //RuleFor(x => x.PositionId)
            //    .MustAsync(IfExistPosition).WithMessage("მიუთითეთ პოზიცია სწორად");
        }

        private async Task<bool> IfExistPosition(Guid positionId, CancellationToken cancellationToken)
        {
            return await _positionRepository.CheckAsync(x => x.Id == positionId);
        }
    }
}