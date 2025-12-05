using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ServiceModel;

namespace Infrastructure.Messaging.Helpers;

public static class WcfServiceConfiguration
{
    public static void AddWcfServiceSingleton<TServiceType, TImplementation>(this IServiceCollection services) =>
        services.AddSingleton(typeof(TServiceType), (serviceProvider) => ImplementationFactory<TImplementation>(serviceProvider)!);

    public static void AddWcfServiceTransient<TServiceType, TImplementation>(this IServiceCollection services) =>
        services.AddTransient(typeof(TServiceType), (serviceProvider) => ImplementationFactory<TImplementation>(serviceProvider)!);

    public static void AddWcfServiceScoped<TServiceType, TImplementation>(this IServiceCollection services) =>
        services.AddScoped(typeof(TServiceType), (serviceProvider) => ImplementationFactory<TImplementation>(serviceProvider)!);


    private static TImplementation ImplementationFactory<TImplementation>(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var config = configuration.GetSection(nameof(ServiceReferenceConfiguration)).Get<ServiceReferenceConfiguration>();

        _ = config ?? throw new ArgumentException("Can't find ServiceReferenceConfiguration. Check appsettings file");

        var type = typeof(TImplementation);

        var addressConfig = config.EndpointConfigs.FirstOrDefault(endpoint => endpoint.Name == type.Name);
        _ = addressConfig ?? throw new ArgumentException($"Can't find endpoint configuration for {type.Name} in service configuration file");

        var bindingConfig = config.BasicHttpBindings.FirstOrDefault(binding => binding.Name == addressConfig.BindingConfiguration);

        _ = bindingConfig ?? throw new ArgumentException($"Can't find binding configuration for {addressConfig.Name} in service configuration file");

        var instance = Activator.CreateInstance(type, bindingConfig, new EndpointAddress(addressConfig.Address));
        _ = instance ?? throw new ArgumentException("Could not create instance");

        return (TImplementation)instance;
    }

    internal class ServiceReferenceConfiguration
    {
        public List<BasicHttpBinding> BasicHttpBindings { get; set; } = null!;
        public List<EndpointConfig> EndpointConfigs { get; set; } = null!;
    }

    internal class EndpointConfig
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Binding { get; set; }
        public string BindingConfiguration { get; set; } = null!;
    }
}