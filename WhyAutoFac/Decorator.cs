using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace WhyAutoFac;

public class Decorator
{
    interface IHandler
    {
        void Handle();
    }

    class SaveHandler : IHandler
    {
        public void Handle()
        {
            Console.WriteLine("Saving");
        }
    }

    class LoggingDecorator : IHandler
    {
        public IHandler Inner { get; }

        public LoggingDecorator(IHandler inner)
        {
            Inner = inner;
        }

        public void Handle()
        {
            Console.WriteLine("Start Handle");
            Inner.Handle();
            Console.WriteLine("End Handle");
        }
    }

    [Fact]
    public void AutoFac_HasFirstClassDecoratorSupport()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<SaveHandler>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        builder.RegisterDecorator<LoggingDecorator, IHandler>();

        var container = builder.Build();
        
        var handler = container.Resolve<IHandler>();
        var asLoggingDecorator = Assert.IsType<LoggingDecorator>(handler);
        Assert.IsType<SaveHandler>(asLoggingDecorator.Inner);

    }

    [Fact]
    public void MEDI_CanDoItManually()
    {
        var services = new ServiceCollection();
        services.AddScoped<SaveHandler>();
        services.AddScoped<IHandler>(ctx => new LoggingDecorator(ctx.GetRequiredService<SaveHandler>()));
        
        var serviceProvider = services.BuildServiceProvider();
        
        var handler = serviceProvider.GetRequiredService<IHandler>();
        var asLoggingDecorator = Assert.IsType<LoggingDecorator>(handler);
        Assert.IsType<SaveHandler>(asLoggingDecorator.Inner);
    }
}
