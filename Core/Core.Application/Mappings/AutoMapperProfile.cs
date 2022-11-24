using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Mappings;
public static class MapperConfig
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig<Employee, GetEmployeeDto>.NewConfig()
            .Map(dest => dest.Gender, src => src.Gender == Gender.Male ? "კაცი" : "ქალი")
            .Map(dest => dest.Age, src => DateTime.Now.Year - src.BirthDate.Year);
    }
}