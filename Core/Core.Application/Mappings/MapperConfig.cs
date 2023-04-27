using Core.Application.DTOs;
using Core.Domain.Enums;
using Core.Domain.Models;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Mappings;
public static class MapperConfig
{
    public static void RegisterMapsterConfiguration(this IServiceCollection _)
    {
        TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

        TypeAdapterConfig<Employee, GetEmployeeDto>.NewConfig()
            .Map(dest => dest.Gender, src => src.Gender == Gender.Male ? "კაცი" : "ქალი")
            .Map(dest => dest.Age, src => DateTime.Now.Year - src.BirthDate.Year);
    }
}