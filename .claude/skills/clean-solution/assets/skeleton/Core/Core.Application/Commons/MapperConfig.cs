using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Commons;

public static class MapperConfig
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

        // Add an explicit map only for members that are not 1:1 by name, e.g.:
        // TypeAdapterConfig<Employee, GetEmployeeDto>.NewConfig()
        //     .Map(dest => dest.Age, src => src.BirthDate.CalculateAge());
    }
}
