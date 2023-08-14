using Autofac;

namespace WhyAutoFac;

public class OverridePerScope
{
    interface IEmailSender
    {
        void Send(string message);
    }
    
    class EmailSender : IEmailSender
    {
        public void Send(string message)
        {
            Console.WriteLine($"Sending email: {message}");
        }
    }
    
    class TestEmailSender : IEmailSender
    {
        List<string> _messages = new();
        public IReadOnlyList<string> SentMessages => _messages;
        
        public void Send(string message)
        {
            _messages.Add(message);
        }
    }
    

    [Fact]
    public void AutoFac_CanOverrideServicesAtScopeLevel()
    {
        // imagine this is in the real app Startup.cs
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterType<EmailSender>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        var container = containerBuilder.Build();
        
        // now in my test I can override the registration
        var testScope = container.BeginLifetimeScope(builder =>
        {
            builder.RegisterType<TestEmailSender>()
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        });
        
        // and the test scope will use the test implementation
        var emailSender = testScope.Resolve<IEmailSender>();
        Assert.IsType<TestEmailSender>(emailSender);
        emailSender.Send("Hello");

        // I can also resolve the same instance under its real concrete type because I used AsSelf()
        // and can now access properties specific to the test double
        var testSender = testScope.Resolve<TestEmailSender>();
        Assert.Single(testSender.SentMessages);
        Assert.Equal("Hello", testSender.SentMessages[0]);

    }

    [Fact]
    public void MEDI_LiterallyCantEven()
    {
        throw new NotImplementedException("MEDI cant edit a service collection once built");
    }
}
