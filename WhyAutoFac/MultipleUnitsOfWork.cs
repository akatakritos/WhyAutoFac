using Autofac;
using Autofac.Features.OwnedInstances;

namespace WhyAutoFac;

public class MultipleUnitsOfWork
{
    class DbContext : IDisposable
    {
        public static int DisposeCount = 0;

        public void Dispose() => DisposeCount++;
        
        public void SaveChanges(){}
    }
    
    class BigImportJob
    {
        private readonly Func<Owned<DbContext>> _getUnitOfWork;

        public BigImportJob(Func<Owned<DbContext>> getUnitOfWork)
        {
            _getUnitOfWork = getUnitOfWork;
        }

        public void Run()
        {
            for (int i = 0; i < 100; i++)
            {
                using var db = _getUnitOfWork();
                // do stuff with it
                db.Value.SaveChanges();
            }
        }
    }

    [Fact]
    public void AutoFac_CanRegisterStuff()
    {
        var builder = new ContainerBuilder();
        
        builder.RegisterType<DbContext>()
            .InstancePerLifetimeScope()
            .AsSelf();
        
        builder.RegisterType<BigImportJob>()
            .AsSelf();

        var container = builder.Build();

        using var scope = container.BeginLifetimeScope();

        var job = scope.Resolve<BigImportJob>();
        job.Run();

        // Even though dbcontext is instance per scope, we can still use it multiple times in the job
        Assert.Equal(100, DbContext.DisposeCount);

    }
}
