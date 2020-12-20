using AutoMapper;
using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Domain.Models;
using CleanSolution.Core.Domain.Enums;
using System;

namespace CleanSolution.Core.Application.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SetPositionDto, Position>();
            CreateMap<Position, GetPositionDto>();

            CreateMap<SetEmployeeDto, Employee>();
            CreateMap<Employee, GetEmployeeDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender == Gender.Male ? "კაცი" : "ქალი"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.BirthDate.Year));

        }
    }
}
