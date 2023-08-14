using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace WhyAutoFac;

public class ReflectionRegistrations
{
    interface IRepository {}
    interface ILogger {}
    class Logger:ILogger{}

    class Repository : IRepository
    {
        public Repository(string connectionString, ILogger logger)
        {
            
        }
    }

    [Fact]
    public void AutoFac_CanProvidePrimitivesAndResolveTheRest()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Logger>()
            .AsImplementedInterfaces()
            .SingleInstance();
        builder.RegisterType<Repository>()
            .WithParameter("connectionString", "Server=.;Database=WhyAutoFac;Trusted_Connection=True;")
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        var container = builder.Build();
        var repository = container.Resolve<IRepository>();
        Assert.IsType<Repository>(repository);
    }

    [Fact]
    public void MEDI_HasToUseLambda()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger, Logger>();
        services.AddScoped<IRepository>(c => new Repository("Server=.;Database=WhyAutoFac;Trusted_Connection=True;", c.GetRequiredService<ILogger>()));
        
        var serviceProvider = services.BuildServiceProvider();
        var repository = serviceProvider.GetService<IRepository>();
        Assert.IsType<Repository>(repository);
    }
}
