using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace WhyAutoFac;

public class UnitTest1
{
    interface IContextResolver
    {
        int GetTenantId();
    }

    interface IContextProvider
    {
        void SetTenantId(int id);
    }

    class ContextHolder : IContextProvider, IContextResolver
    {
        private int _tenantId;
        public void SetTenantId(int id) => _tenantId = id;
        public int GetTenantId() => _tenantId;
    }
    
    
    [Fact]
    public void AutoFac_RegistersSameInstanceUnderBothServices()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ContextHolder>()
            .As<IContextProvider>()
            .As<IContextResolver>()
            .InstancePerLifetimeScope();
        
        var container = builder.Build();
        var contextProvider = container.Resolve<IContextProvider>();
        var contextResolver = container.Resolve<IContextResolver>();
        Assert.Same(contextProvider, contextResolver);
    }

    [Fact]
    public void MEDI_DoesNotRegisterSameInstance()
    {
        var services = new ServiceCollection();
        services.AddScoped<IContextProvider, ContextHolder>();
        services.AddScoped<IContextResolver, ContextHolder>();
        var serviceProvider = services.BuildServiceProvider();
        
        var contextProvider = serviceProvider.GetService<IContextProvider>();
        var contextResolver = serviceProvider.GetService<IContextResolver>();
        Assert.NotSame(contextProvider, contextResolver);
    }

    [Fact]
    public void MEDI_YouCanGetSameInstancesViaLambdas()
    {
        
        var services = new ServiceCollection();
        services.AddScoped<ContextHolder>();
        services.AddScoped<IContextProvider>(c => c.GetRequiredService<ContextHolder>());
        services.AddScoped<IContextResolver>(c => c.GetRequiredService<ContextHolder>());
        var serviceProvider = services.BuildServiceProvider();
        
        var contextProvider = serviceProvider.GetService<IContextProvider>();
        var contextResolver = serviceProvider.GetService<IContextResolver>();
        Assert.Same(contextProvider, contextResolver);
    }
}
