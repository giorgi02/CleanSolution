using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;

namespace CleanSolution.Core.Application.Mappings;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap(typeof(Pagination<>), typeof(Pagination<>));

        CreateMap<SetPositionDto, Position>();
        CreateMap<SetEmployeeDto, Employee>();

        CreateMap<Position, GetPositionDto>();
        CreateMap<Employee, GetEmployeeDto>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender == Gender.Male ? "კაცი" : "ქალი"))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.BirthDate.Year));
    }
}
