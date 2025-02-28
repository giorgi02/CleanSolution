using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Commons;
public static class MapperConfig
{
    public static void RegisterMapsterConfiguration(this IServiceCollection _)
    {
        TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);
    }
}