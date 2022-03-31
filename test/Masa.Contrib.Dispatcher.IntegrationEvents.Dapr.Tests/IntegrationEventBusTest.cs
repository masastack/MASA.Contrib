namespace Masa.Contrib.Dispatcher.IntegrationEvents.Dapr.Tests;

[TestClass]
public class IntegrationEventBusTest
{
    private Mock<IDispatcherOptions> _options;
    private Mock<IOptions<DispatcherOptions>> _dispatcherOptions;
    private Mock<DaprClient> _daprClient;
    private Mock<ILogger<IntegrationEventBus>> _logger;
    private Mock<IIntegrationEventLogService> _eventLog;
    private Mock<IOptionsMonitor<AppConfig>> _appConfig;
    private Mock<IEventBus> _eventBus;
    private Mock<IUnitOfWork> _uoW;

    [TestInitialize]
    public void Initialize()
    {
        _options = new();
        _options.Setup(option => option.Services).Returns(new ServiceCollection()).Verifiable();
        _dispatcherOptions = new();
        _dispatcherOptions
            .Setup(option => option.Value)
            .Returns(() => new DispatcherOptions(_options.Object.Services, AppDomain.CurrentDomain.GetAssemblies()));
        _daprClient = new();
        _logger = new();
        _eventLog = new();
        _eventLog.Setup(eventLog => eventLog.SaveEventAsync(It.IsAny<IIntegrationEvent>(), null!)).Verifiable();
        _eventLog.Setup(eventLog => eventLog.MarkEventAsInProgressAsync(It.IsAny<Guid>())).Verifiable();
        _eventLog.Setup(eventLog => eventLog.MarkEventAsPublishedAsync(It.IsAny<Guid>())).Verifiable();
        _eventLog.Setup(eventLog => eventLog.MarkEventAsFailedAsync(It.IsAny<Guid>())).Verifiable();
        _appConfig = new();
        _appConfig.Setup(appConfig => appConfig.CurrentValue).Returns(() => new AppConfig()
        {
            AppId = "Test"
        });
        _eventBus = new();
        _uoW = new();
        _uoW.Setup(uoW => uoW.CommitAsync(default)).Verifiable();
        _uoW.Setup(uoW => uoW.Transaction).Returns(() => null!);
        _uoW.Setup(uoW => uoW.UseTransaction).Returns(true);
    }

    [TestMethod]
    public void TestDispatcherOption()
    {
        var services = new ServiceCollection();
        DispatcherOptions options;

        Assert.ThrowsException<ArgumentException>(() =>
        {
            options = new DispatcherOptions(services, null!);
        });
        Assert.ThrowsException<ArgumentException>(() =>
        {
            options = new DispatcherOptions(services, Array.Empty<Assembly>());
        });
        options = new DispatcherOptions(services, new[] { typeof(IntegrationEventBusTest).Assembly });
        Assert.IsTrue(options.Services.Equals(services));
        var allEventTypes = new[] { typeof(IntegrationEventBusTest).Assembly }.SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && type != typeof(IntegrationEvent) && typeof(IEvent).IsAssignableFrom(type)).ToList();
        Assert.IsTrue(options.AllEventTypes.Count == allEventTypes.Count());
    }

    [TestMethod]
    public void TestAddMultDaprEventBus()
    {
        var services = new ServiceCollection();
        Mock<IDistributedDispatcherOptions> distributedDispatcherOptions = new();
        distributedDispatcherOptions.Setup(option => option.Services).Returns(services).Verifiable();
        distributedDispatcherOptions.Setup(option => option.Assemblies).Returns(AppDomain.CurrentDomain.GetAssemblies()).Verifiable();
        distributedDispatcherOptions.Object
            .UseDaprEventBus<CustomizeIntegrationEventLogService>()
            .UseDaprEventBus<CustomizeIntegrationEventLogService>();
        var serviceProvider = services.BuildServiceProvider();
        Assert.IsTrue(serviceProvider.GetServices<IIntegrationEventBus>().Count() == 1);
    }

    [TestMethod]
    public void TestAddDaprEventBus()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDaprEventBus<CustomizeIntegrationEventLogService>();
        var serviceProvider = services.BuildServiceProvider();
        var integrationEventBus = serviceProvider.GetRequiredService<IIntegrationEventBus>();
        Assert.IsNotNull(integrationEventBus);
    }

    [TestMethod]
    public void TestNotUseLoggerAndUoW()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLogging();
        services
            .AddDaprEventBus<
                CustomizeIntegrationEventLogService>(); //The logger cannot be mocked and cannot verify that the logger is executed only once

        var serviceProvider = services.BuildServiceProvider();
        var integrationEventBus = serviceProvider.GetRequiredService<IIntegrationEventBus>();
        Assert.IsNotNull(integrationEventBus);
    }

    [TestMethod]
    public void TestUseLogger()
    {
        IServiceCollection services = new ServiceCollection();


        services.AddDaprEventBus<CustomizeIntegrationEventLogService>(AppDomain.CurrentDomain.GetAssemblies(), option =>
        {
            option.PubSubName = "pubsub";
        });
        var serviceProvider = services.BuildServiceProvider();
        var integrationEventBus = serviceProvider.GetRequiredService<IIntegrationEventBus>();
        Assert.IsNotNull(integrationEventBus);
    }

    [TestMethod]
    public void TestAddDaprEventBusAndNullServicesAsync()
    {
        IServiceCollection services = null!;
        Mock<IDistributedDispatcherOptions> distributedDispatcherOptions = new();
        distributedDispatcherOptions.Setup(option => option.Services).Returns(services).Verifiable();
        distributedDispatcherOptions.Setup(option => option.Assemblies).Returns(AppDomain.CurrentDomain.GetAssemblies()).Verifiable();
        Assert.ThrowsException<ArgumentNullException>(() =>
                distributedDispatcherOptions.Object.UseDaprEventBus<CustomizeIntegrationEventLogService>(),
            $"Value cannot be null. (Parameter '{nameof(_options.Object.Services)}')");
    }

    [TestMethod]
    public async Task TestPublishIntegrationEventAsync()
    {
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            _eventBus.Object,
            _uoW.Object);
        RegisterUserIntegrationEvent @event = new RegisterUserIntegrationEvent()
        {
            Account = "lisa",
            Password = "123456"
        };
        _daprClient.Setup(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default))
            .Verifiable();
        await integrationEventBus.PublishAsync(@event);

        _daprClient.Verify(dapr => dapr.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default),
            Times.Once);
    }

    [TestMethod]
    public async Task TestNotUseUoWAndLoggerAsync()
    {
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            null,
            _eventBus.Object);
        RegisterUserIntegrationEvent @event = new RegisterUserIntegrationEvent()
        {
            Account = "lisa",
            Password = "123456"
        };
        _daprClient.Setup(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default))
            .Verifiable();
        await integrationEventBus.PublishAsync(@event);

        _eventLog.Verify(eventLog => eventLog.MarkEventAsInProgressAsync(@event.Id), Times.Never);
        _daprClient.Verify(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default),
            Times.Once);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsPublishedAsync(@event.Id), Times.Never);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsFailedAsync(@event.Id), Times.Never);
    }

    [TestMethod]
    public async Task TestNotUseTransactionAsync()
    {
        _uoW.Setup(uoW => uoW.UseTransaction).Returns(false);
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            _eventBus.Object,
            _uoW.Object);
        RegisterUserIntegrationEvent @event = new RegisterUserIntegrationEvent()
        {
            Account = "lisa",
            Password = "123456"
        };
        _daprClient.Setup(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default))
            .Verifiable();
        await integrationEventBus.PublishAsync(@event);

        _eventLog.Verify(eventLog => eventLog.MarkEventAsInProgressAsync(@event.Id), Times.Never);
        _daprClient.Verify(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default),
            Times.Once);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsPublishedAsync(@event.Id), Times.Never);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsFailedAsync(@event.Id), Times.Never);
    }

    [TestMethod]
    public async Task TestUseTranscationAndNotUseLoggerAsync()
    {
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            null,
            _eventBus.Object,
            _uoW.Object);
        RegisterUserIntegrationEvent @event = new RegisterUserIntegrationEvent()
        {
            Account = "lisa",
            Password = "123456"
        };
        _daprClient.Setup(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default))
            .Verifiable();
        await integrationEventBus.PublishAsync(@event);

        _eventLog.Verify(eventLog => eventLog.MarkEventAsInProgressAsync(@event.Id), Times.Once);
        _daprClient.Verify(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default),
            Times.Once);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsPublishedAsync(@event.Id), Times.Once);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsFailedAsync(@event.Id), Times.Never);
    }

    [TestMethod]
    public async Task TestSaveEventFailedAndNotUseLoggerAsync()
    {
        _eventLog.Setup(eventLog => eventLog.SaveEventAsync(It.IsAny<IIntegrationEvent>(), null!))
            .Callback(() => throw new Exception("custom exception"));
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            null,
            _eventBus.Object,
            _uoW.Object);
        RegisterUserIntegrationEvent @event = new RegisterUserIntegrationEvent()
        {
            Account = "lisa",
            Password = "123456"
        };
        _daprClient.Setup(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default))
            .Verifiable();
        await integrationEventBus.PublishAsync(@event);

        _eventLog.Verify(eventLog => eventLog.MarkEventAsInProgressAsync(@event.Id), Times.Never);
        _daprClient.Verify(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default),
            Times.Never);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsPublishedAsync(@event.Id), Times.Never);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsFailedAsync(@event.Id), Times.Once);
    }

    [TestMethod]
    public async Task TestPublishIntegrationEventAndFailedAsync()
    {
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            _eventBus.Object,
            _uoW.Object);
        RegisterUserIntegrationEvent @event = new RegisterUserIntegrationEvent()
        {
            Account = "lisa",
            Password = "123456"
        };
        _eventLog.Setup(eventLog => eventLog.MarkEventAsPublishedAsync(It.IsAny<Guid>())).Throws<Exception>();
        _daprClient.Setup(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default))
            .Verifiable();
        await integrationEventBus.PublishAsync(@event);

        _eventLog.Verify(eventLog => eventLog.MarkEventAsInProgressAsync(@event.Id), Times.Once);
        _daprClient.Verify(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default),
            Times.Once);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsPublishedAsync(@event.Id), Times.Once);
        _eventLog.Verify(eventLog => eventLog.MarkEventAsFailedAsync(@event.Id), Times.Once);
    }

    [TestMethod]
    public async Task TestPublishIntegrationEventAndNotUoWAsync()
    {
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            _eventBus.Object,
            _uoW.Object);
        RegisterUserIntegrationEvent @event = new RegisterUserIntegrationEvent()
        {
            Account = "lisa",
            Password = "123456",
            UnitOfWork = _uoW.Object
        };
        _daprClient.Setup(client => client.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default))
            .Verifiable();
        await integrationEventBus.PublishAsync(@event);

        _daprClient.Verify(dapr => dapr.PublishEventAsync(_dispatcherOptions.Object.Value.PubSubName, @event.Topic, @event, default),
            Times.Once);
    }

    [TestMethod]
    public async Task TestPublishEventAsync()
    {
        _eventBus.Setup(eventBus => eventBus.PublishAsync(It.IsAny<CreateUserEvent>())).Verifiable();
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            _eventBus.Object,
            _uoW.Object);
        CreateUserEvent @event = new CreateUserEvent()
        {
            Name = "Tom"
        };
        await integrationEventBus.PublishAsync(@event);

        _eventBus.Verify(eventBus => eventBus.PublishAsync(It.IsAny<CreateUserEvent>()), Times.Once);
    }

    [TestMethod]
    public async Task TestPublishEventAndNotEventBusAsync()
    {
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            null,
            _uoW.Object);
        CreateUserEvent @event = new CreateUserEvent()
        {
            Name = "Tom"
        };
        await Assert.ThrowsExceptionAsync<NotSupportedException>(async () =>
        {
            await integrationEventBus.PublishAsync(@event);
        });
    }

    [TestMethod]
    public async Task TestCommitAsync()
    {
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            _eventBus.Object,
            _uoW.Object);

        await integrationEventBus.CommitAsync(default);
        _uoW.Verify(uoW => uoW.CommitAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task TestNotUseUowCommitAsync()
    {
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            _eventBus.Object,
            null);

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await integrationEventBus.CommitAsync());
    }

    [TestMethod]
    public void TestGetAllEventTypes()
    {
        _dispatcherOptions
            .Setup(option => option.Value)
            .Returns(() => new DispatcherOptions(_options.Object.Services, new[] { typeof(IntegrationEventBusTest).Assembly }));
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            null,
            null);

        Assert.IsTrue(integrationEventBus.GetAllEventTypes().Count() == _dispatcherOptions.Object.Value.AllEventTypes.Count());
    }


    [TestMethod]
    public void TestUseEventBusGetAllEventTypes()
    {
        var defaultAssembly = new System.Reflection.Assembly[1] { typeof(IntegrationEventBusTest).Assembly };
        _dispatcherOptions
            .Setup(option => option.Value)
            .Returns(() => new DispatcherOptions(_options.Object.Services, defaultAssembly));
        var allEventType = defaultAssembly
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && typeof(IEvent).IsAssignableFrom(type))
            .ToList();
        _eventBus.Setup(eventBus => eventBus.GetAllEventTypes()).Returns(() => allEventType).Verifiable();
        var integrationEventBus = new IntegrationEventBus(
            _dispatcherOptions.Object,
            _daprClient.Object,
            _eventLog.Object,
            _appConfig.Object,
            _logger.Object,
            _eventBus.Object,
            null);

        Assert.IsTrue(integrationEventBus.GetAllEventTypes().Count() == _dispatcherOptions.Object.Value.AllEventTypes.Count());
        Assert.IsTrue(integrationEventBus.GetAllEventTypes().Count() == allEventType.Count());
    }
}
