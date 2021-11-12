using AutoMapper;
using CleanSolution.Core.Application.Commons;
using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Features.Employees.Commands;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CleanSolution.Core.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SetPositionDto, Position>();
            CreateMap<SetEmployeeDto, Employee>();
            CreateMap<CreateEmployeeCommand.Request, Employee>()
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => LanguagesToLanguage(src.Languages)));

            CreateMap<UpdateEmployeeCommand.Request, Employee>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => LanguagesToLanguage(src.Languages)));


            CreateMap(typeof(Pagination<>), typeof(GetPaginationDto<>));
            CreateMap<Position, GetPositionDto>();
            CreateMap<Employee, GetEmployeeDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender == Gender.Male ? "კაცი" : "ქალი"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.BirthDate.Year));
        }

        private Language LanguagesToLanguage(ICollection<Language> languages)
        {
            Language language = Language.None;
            foreach (var item in languages)
                language |= item;
            return language;
        }
    }
}
