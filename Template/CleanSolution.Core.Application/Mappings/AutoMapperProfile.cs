using AutoMapper;
using $safeprojectname$.Commons;
using $safeprojectname$.DTOs;
using $safeprojectname$.Features.Employees.Commands;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;
using System;

namespace $safeprojectname$.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SetPositionDto, Position>();
            CreateMap<SetEmployeeDto, Employee>();
            CreateMap<CreateEmployeeCommand.Request, Employee>();
            CreateMap<UpdateEmployeeCommand.Request, Employee>();


            CreateMap(typeof(Pagination<>), typeof(GetPaginationDto<>));
            CreateMap<Position, GetPositionDto>();
            CreateMap<Employee, GetEmployeeDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender == Gender.Male ? "კაცი" : "ქალი"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.BirthDate.Year));
        }
    }
}
