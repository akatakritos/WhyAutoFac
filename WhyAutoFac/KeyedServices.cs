using Autofac;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Indexed;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Sdk;

namespace WhyAutoFac;

public class KeyedServices
{
    interface IPushNotifier
    {
        void Notify(string message);
    }
    
    class GooglePushNotifier : IPushNotifier
    {
        public void Notify(string message)
        {
            Console.WriteLine($"Google: {message}");
        }
    }

    class ApplePushNotifier : IPushNotifier
    {
        public void Notify(string message)
        {
            Console.WriteLine($"Apple: {message}");
        }
    }

    [Fact]
    public void AutoFac_CanRegisterUnderKeys()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<GooglePushNotifier>()
            .Keyed<IPushNotifier>("google");
        builder.RegisterType<ApplePushNotifier>()
            .Keyed<IPushNotifier>("apple");
        
        var container = builder.Build();
        var apple = container.ResolveKeyed<IPushNotifier>("apple");
        var google = container.ResolveKeyed<IPushNotifier>("google");

        Assert.IsType<ApplePushNotifier>(apple);
        Assert.IsType<GooglePushNotifier>(google);
    }

    [Fact]
    public void MEDI_LiterallyCantEvenYet()
    {
        throw new NotImplementedException("Coming in .NET 8");
    }

    class GooglePushConsumer
    {
        public IPushNotifier Notifier { get; }

        public GooglePushConsumer([KeyFilter("google")]IPushNotifier notifier)
        {
            Notifier = notifier;
        }
    }

    [Fact]
    public void AutoFac_CanAskForKeyedInstance()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<GooglePushNotifier>()
            .Keyed<IPushNotifier>("google");
        builder.RegisterType<ApplePushNotifier>()
            .Keyed<IPushNotifier>("apple");
        builder.RegisterType<GooglePushConsumer>()
            .WithAttributeFiltering();

        var container = builder.Build();
        var googleConsumer = container.Resolve<GooglePushConsumer>();
        Assert.IsType<GooglePushNotifier>(googleConsumer.Notifier);
    }

    [Fact]
    public void AutoFac_CanProvideKeyedInstanceThroughRegistration()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<GooglePushNotifier>()
            .Keyed<IPushNotifier>("google");
        builder.RegisterType<ApplePushNotifier>()
            .Keyed<IPushNotifier>("apple");
        builder.RegisterType<GooglePushConsumer>()
            .WithParameter(new ResolvedParameter(
                (p, _) => p.ParameterType == typeof(IPushNotifier),
                (_, ctx) => ctx.ResolveKeyed<IPushNotifier>("google")));

        var container = builder.Build();
        var googleConsumer = container.Resolve<GooglePushConsumer>();
        Assert.IsType<GooglePushNotifier>(googleConsumer.Notifier); 
    }
    
    [Fact]
    public void MEDI_CanProvideParticularImplementationsAtRegistrationTime()
    {
        var services = new ServiceCollection();
        services.AddTransient<GooglePushNotifier>();
        services.AddTransient<ApplePushNotifier>();
        services.AddTransient<GooglePushConsumer>(ctx =>
            new GooglePushConsumer(ctx.GetRequiredService<GooglePushNotifier>()));
        
        var servicesProvider = services.BuildServiceProvider();
        var googleConsumer = servicesProvider.GetRequiredService<GooglePushConsumer>();
        Assert.IsType<GooglePushNotifier>(googleConsumer.Notifier);
    }
    

    class NotificationService
    {
        public IIndex<string, IPushNotifier> Strategies { get; }

        public NotificationService(IIndex<string, IPushNotifier> strategies)
        {
            Strategies = strategies;
        }

        public void Notify(string platform, string message)
        {
            Strategies[platform].Notify(message);
        }
    }
    
    [Fact]
    public void AutoFac_CanUseIIndexToDoStrategyPattern()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<GooglePushNotifier>()
            .Keyed<IPushNotifier>("google");
        builder.RegisterType<ApplePushNotifier>()
            .Keyed<IPushNotifier>("apple");
        builder.RegisterType<NotificationService>();

        var container = builder.Build();
        var service = container.Resolve<NotificationService>();
        service.Notify("google", "Hello, Google!");
        service.Notify("apple", "Hello, Apple!");
    }
    
    interface IPushStrategy {
        string Name { get; }
        void Notify(string message);
    }
    
    class GooglePushStrategy : IPushStrategy
    {
        public string Name => "google";
        public void Notify(string message)
        {
            Console.WriteLine($"Google: {message}");
        }
    }
    
    class ApplePushStrategy : IPushStrategy
    {
        public string Name => "apple";
        public void Notify(string message)
        {
            Console.WriteLine($"Apple: {message}");
        }
    }

    class MediPushService
    {
        public IEnumerable<IPushStrategy> Strategies { get; }

        public MediPushService(IEnumerable<IPushStrategy> strategies)
        {
            Strategies = strategies;
        }

        public void Notify(string platform, string message)
        {
            Strategies.First(s => s.Name == platform).Notify(message);
        }
    }

    [Fact]
    public void MEDI_CanDoStrategyPatternMoreExplicitly()
    {
        var services = new ServiceCollection();
        services.AddTransient<IPushStrategy, GooglePushStrategy>();
        services.AddTransient<IPushStrategy, ApplePushStrategy>();
        services.AddTransient<MediPushService>();
        
        var servicesProvider = services.BuildServiceProvider();
        var service = servicesProvider.GetRequiredService<MediPushService>();
        service.Notify("google", "Hello, Google!");
    }
    
}
