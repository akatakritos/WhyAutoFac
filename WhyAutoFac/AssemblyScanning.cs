using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace WhyAutoFac.AssemblyScanningDemo;

public interface ISampleRepository
{
}

public class SampleRepository: ISampleRepository{}

public class AssemblyScanning
{
    [Fact]
    public void AutoFac_ScanAndFilter()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAssemblyTypes(typeof(AssemblyScanning).Assembly)
            .PublicOnly()
            .Where(t => t.Namespace == "WhyAutoFac.AssemblyScanningDemo")
            .Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces();
        
        var container = builder.Build();
        var repository = container.Resolve<ISampleRepository>();
        Assert.IsType<SampleRepository>(repository);
    }

    [Fact]
    public void MEDI_ScanItYourself()
    {
        var services = new ServiceCollection();
        
        var repositories = typeof(AssemblyScanning).Assembly
            .GetTypes()
            .Where(t => t.IsPublic)
            .Where(t => t.Namespace == "WhyAutoFac.AssemblyScanningDemo")
            .Where(t => t.Name.EndsWith("Repository"));
        
        foreach (var repositoryType in repositories)
        {
            foreach (var interfaceType in repositoryType.GetInterfaces())
            {
                services.AddScoped(interfaceType, repositoryType);
            }
        }
        
        var serviceProvider = services.BuildServiceProvider();
        var repository = serviceProvider.GetService<ISampleRepository>();
        Assert.IsType<SampleRepository>(repository);

    }
}
