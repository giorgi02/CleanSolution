using Core.Domain.Entities;

namespace Core.Application.DTOs;
public class GetEmployeeDto
{
    public Guid Id { get; set; }
    public string? PrivateNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public int Age { get; set; }
    public string[]? Phones { get; set; }
    public Address? Address { get; set; }
    public GetPositionDto? Position { get; set; }
}