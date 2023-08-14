using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace WhyAutoFac;

interface IService
{
}

class Service : IService
{
}

class FeatureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Service>().As<IService>();
    }
}

public static class FeatureModuleServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureModule(this IServiceCollection services)
    {
        services.AddScoped<IService, Service>();
        return services;
    }
}

public class Modules
{
    [Fact]
    public void AutoFac_CanRegisterModules()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<FeatureModule>();
        var container = builder.Build();

        var service = container.Resolve<IService>();
        Assert.IsType<Service>(service);
    }

    [Fact]
    public void MEDI_CanDoModulesViaExtensionMethods()
    {
        var services = new ServiceCollection();
        services.AddFeatureModule();
        var serviceProvider = services.BuildServiceProvider();

        var service = serviceProvider.GetService<IService>();
        Assert.IsType<Service>(service);
    }
}
