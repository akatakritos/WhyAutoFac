using Autofac;

namespace WhyAutoFac;

public class NamedScopes
{
    interface IEventPublisher
    {
        void Publish(object evt);
    }

    class TestEventPublisher: IEventPublisher
    {
        public List<object> PublishedEvents { get; } = new List<object>();
        
        public void Publish(object evt)
        {
            PublishedEvents.Add(evt);
        }
    }
    
    interface IUnitOfWork
    {
    }

    class UnitOfWork : IUnitOfWork
    {
    }
    
    class Request1
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventPublisher;

        public Request1(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
        {
            _unitOfWork = unitOfWork;
            _eventPublisher = eventPublisher;
        }
        
        public void DoSomething()
        {
            _eventPublisher.Publish("Hello");
        }
    }

    class Request2
    {
        record RequestEvent2();
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventPublisher;

        public Request2(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
        {
            _unitOfWork = unitOfWork;
            _eventPublisher = eventPublisher;
        }
        
        public void DoSomething()
        {
            _eventPublisher.Publish(new RequestEvent2());
        }
        
    }

    [Fact]
    public void AutoFac_NamedScope()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<UnitOfWork>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterType<Request1>().AsSelf();
        builder.RegisterType<Request2>().AsSelf();
        
        // I dont want this to be a singleton, because I want it to be disposed at the end of
        // and not leak data between tests
        builder.RegisterType<TestEventPublisher>()
            .InstancePerMatchingLifetimeScope("CURRENT_TEST")
            .AsImplementedInterfaces()
            .AsSelf();
        
        var container = builder.Build();

        using (var testScope = container.BeginLifetimeScope("CURRENT_TEST"))
        {
            // we want each request to run in its own scope because that's how the real app works
            using (var request1Scope = testScope.BeginLifetimeScope())
            {
                request1Scope.Resolve<Request1>().DoSomething();
            }
            
            using (var request2Scope = testScope.BeginLifetimeScope())
            {
                request2Scope.Resolve<Request2>().DoSomething();
            }
            
            var testEventPublisher = testScope.Resolve<TestEventPublisher>();
            Assert.Equal(2, testEventPublisher.PublishedEvents.Count);
        }
    }

    [Fact]
    public void MEDI_LiterallyCantEven()
    {
        throw new NotImplementedException("MEDI only supports singleton and transient scopes");
    }
}
