using Core.Application.Commons;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Mappings;
public sealed class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap(typeof(Pagination<>), typeof(Pagination<>));

        CreateMap<Position, GetPositionDto>();
        CreateMap<Employee, GetEmployeeDto>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender == Gender.Male ? "კაცი" : "ქალი"))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.BirthDate.Year));
    }
}