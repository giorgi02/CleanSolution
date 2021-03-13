﻿using AutoMapper;
using CleanSolution.Core.Application.DTOs;
using CleanSolution.Core.Application.Features.Positions.Commands;
using CleanSolution.Core.Domain.Entities;
using CleanSolution.Core.Domain.Enums;
using System;

namespace CleanSolution.Core.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreatePositionCommand.Request, Position>();
            CreateMap<SetPositionDto, Position>();
            CreateMap<Position, GetPositionDto>();

            CreateMap<SetEmployeeDto, Employee>();
            CreateMap<Employee, GetEmployeeDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender == Gender.Male ? "კაცი" : "ქალი"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.Now.Year - src.BirthDate.Year));

        }
    }
}
