using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace WhyAutoFac;

public class FuncRegistrations
{
    interface IExpensiveService
    {
    }
    
    class ExpensiveService : IExpensiveService
    {
        public ExpensiveService()
        {
            Thread.Sleep(500);
        }
    }

    class MightNotNeedExpensiveService
    {
        private readonly Func<IExpensiveService> _expensiveServiceFactory;

        public MightNotNeedExpensiveService(Func<IExpensiveService> expensiveServiceFactory)
        {
            _expensiveServiceFactory = expensiveServiceFactory;
        }

        public void MaybeUseService()
        {
            if (Random.Shared.NextDouble() > 0.5)
            {
                var expensiveService = _expensiveServiceFactory();
                // do something with expensive service
            }
        }
    }

    [Fact]
    public void AutoFac_AutomaticallySupportsFuncT()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<ExpensiveService>().As<IExpensiveService>();
        builder.RegisterType<MightNotNeedExpensiveService>();
        var container = builder.Build();

        container.Resolve<MightNotNeedExpensiveService>();
    }
    
    [Fact]
    public void MEDI_YouHaveToRegisterFuncT()
    {
        var services = new ServiceCollection();
        services.AddScoped<IExpensiveService, ExpensiveService>();
        services.AddScoped<MightNotNeedExpensiveService>();
        var serviceProvider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => { serviceProvider.GetService<MightNotNeedExpensiveService>(); });
    }

    [Fact]
    public void MEDI_ButYouCanDoItManually()
    {
        
        var services = new ServiceCollection();
        services.AddScoped<IExpensiveService, ExpensiveService>();
        services.AddScoped<MightNotNeedExpensiveService>();
        services.AddSingleton<Func<IExpensiveService>>(c => new Func<IExpensiveService>(
            () => c.GetRequiredService<IExpensiveService>()));
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<MightNotNeedExpensiveService>();
    }
}
