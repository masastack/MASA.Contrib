// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.Ddd.Domain.Events;
using Microsoft.Extensions.Options;

namespace Masa.Contrib.Data.UoW.EF.Tests;

[TestClass]
public class TestUnitOfWork : TestBase
{
    private Mock<IDispatcherOptions> _options;

    [TestInitialize]
    public void Initialize()
    {
        _options = new();
        _options.Setup(option => option.Services).Returns(new ServiceCollection()).Verifiable();
    }

    [TestMethod]
    public void TestAddUoWAndNullServices()
    {
        var options = new Mock<IDispatcherOptions>();
        Assert.ThrowsException<ArgumentNullException>(() => options.Object.UseUoW<CustomDbContext>());
    }

    [TestMethod]
    public void TestAddUoWAndUseSqlLite()
    {
        _options.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite(_connectionString));
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        Assert.IsNotNull(serviceProvider.GetRequiredService<CustomDbContext>());
    }

    [TestMethod]
    public void TestAddMultUoW()
    {
        _options.Object
            .UseUoW<CustomDbContext>(options => options.UseTestSqlite(_connectionString))
            .UseUoW<CustomDbContext>(options => options.UseTestSqlite(_connectionString));

        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        Assert.IsTrue(serviceProvider.GetServices<IUnitOfWork>().Count() == 1);
    }

    [TestMethod]
    public void TestTransaction()
    {
        Mock<IUnitOfWork> uoW = new();
        Assert.IsTrue(new Transaction(uoW.Object).UnitOfWork!.Equals(uoW.Object));
    }

    [TestMethod]
    public async Task TestUseTranscationAsync()
    {
        _options.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite(Connection));
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<CustomDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var uoW = serviceProvider.GetRequiredService<IUnitOfWork>();

        var transaction = uoW.Transaction;
        Users user = new Users()
        {
            Name = Guid.NewGuid().ToString()
        };
        dbContext.Add(user);
        await uoW.SaveChangesAsync();
        await uoW.RollbackAsync();

        Assert.IsTrue(!dbContext.User.ToList().Any());
    }

    [TestMethod]
    public async Task TestNotUseTranscationAsync()
    {
        _options.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite(Connection));
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<CustomDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var uoW = new UnitOfWork<CustomDbContext>(serviceProvider);

        Users user = new Users()
        {
            Name = Guid.NewGuid().ToString()
        };
        dbContext.Add(user);
        await uoW.SaveChangesAsync();
        await Assert.ThrowsExceptionAsync<NotSupportedException>(async () => await uoW.RollbackAsync());
    }

    [TestMethod]
    public async Task TestNotTransactionCommitAsync()
    {
        _options.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite(_connectionString));
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<CustomDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var uoW = new UnitOfWork<CustomDbContext>(serviceProvider);
        await Assert.ThrowsExceptionAsync<NotSupportedException>(async () => await uoW.CommitAsync());
    }

    [TestMethod]
    public async Task TestCommitAsync()
    {
        _options.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite(Connection));
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<CustomDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var uoW = new UnitOfWork<CustomDbContext>(serviceProvider);
        var user = new Users()
        {
            Name = "Tom"
        };
        var transcation = uoW.Transaction;
        dbContext.User.Add(user);
        await uoW.SaveChangesAsync();
        await uoW.CommitAsync();

        Assert.IsTrue(dbContext.User.ToList().Count == 1);
    }

    [TestMethod]
    public async Task TestOpenRollbackAsync()
    {
        _options.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite(Connection));
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<CustomDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var uoW = serviceProvider.GetRequiredService<IUnitOfWork>();
        var user = new Users();
        var transcation = uoW.Transaction;
        dbContext.User.Add(user);
        await uoW.CommitAsync();

        Assert.IsTrue(!await dbContext.User.AnyAsync());
    }

    [TestMethod]
    public async Task TestAddLoggerAndOpenRollbackAsync()
    {
        _options.Object.Services.AddLogging();
        _options.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite(Connection));
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<CustomDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var uoW = serviceProvider.GetRequiredService<IUnitOfWork>();
        var user = new Users();
        var transcation = uoW.Transaction;
        dbContext.User.Add(user);
        await uoW.CommitAsync();

        Assert.IsTrue(!await dbContext.User.AnyAsync());
    }

    [TestMethod]
    public void TestDataConnectionString()
    {
        IConfiguration configuration = new ConfigurationManager();
        _options.Object.Services.AddSingleton(_ => configuration);
        _options.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite(Connection));
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        var dataConnectionStringProvider = serviceProvider.GetRequiredService<IDbConnectionStringProvider>();
        Assert.IsTrue(dataConnectionStringProvider.DbContextOptionsList.Count == 1 &&
            dataConnectionStringProvider.DbContextOptionsList.Any(option => option.ConnectionString == _connectionString));
    }

    [TestMethod]
    public void TestUnitOfWorkManager()
    {
        _options.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite(Connection));
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        var dbContext = serviceProvider.GetRequiredService<CustomDbContext>();
        var dbContext2 = serviceProvider.GetRequiredService<CustomDbContext>();
        Assert.IsTrue(dbContext.Equals(dbContext2));

        var newUnitOfWork =
            unitOfWorkManager.CreateDbContext(
                new MasaDbContextConfigurationOptions(_connectionString));
        Assert.IsFalse(newUnitOfWork.Equals(unitOfWork));
        var newDbContext = newUnitOfWork.ServiceProvider.GetRequiredService<CustomDbContext>();
        Assert.IsFalse(dbContext.Equals(newDbContext));

        Assert.ThrowsException<ArgumentException>(()
            => unitOfWorkManager.CreateDbContext(new MasaDbContextConfigurationOptions("")));
    }

    [TestMethod]
    public async Task TestUnitOfWorkAccessorAsync()
    {
        var services = new ServiceCollection();
        services.Configure<MasaDbConnectionOptions>(options =>
        {
            options.ConnectionStrings = new ConnectionStrings()
            {
                DefaultConnection = _connectionString
            };
        });
        _options.Setup(option => option.Services).Returns(services).Verifiable();
        _options.Object.UseUoW<CustomDbContext>(options => options.UseSqlite());
        var serviceProvider = _options.Object.Services.BuildServiceProvider();
        var unitOfWorkAccessor = serviceProvider.GetService<IUnitOfWorkAccessor>();
        Assert.IsTrue(unitOfWorkAccessor is { CurrentDbContextOptions: null });
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        Assert.IsNotNull(unitOfWork);
        Assert.IsTrue(!unitOfWork.TransactionHasBegun);
        unitOfWorkAccessor = serviceProvider.GetService<IUnitOfWorkAccessor>();
        Assert.IsTrue(unitOfWorkAccessor!.CurrentDbContextOptions != null && unitOfWorkAccessor.CurrentDbContextOptions.ConnectionString ==
            _connectionString);

        var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
        var unitOfWorkNew = unitOfWorkManager.CreateDbContext(false);
        var unitOfWorkAccessorNew = unitOfWorkNew.ServiceProvider.GetService<IUnitOfWorkAccessor>();
        Assert.IsTrue(unitOfWorkAccessorNew!.CurrentDbContextOptions != null &&
            unitOfWorkAccessorNew.CurrentDbContextOptions.ConnectionString ==
            _connectionString);

        var unitOfWorkNew2 =
            unitOfWorkManager.CreateDbContext(new MasaDbContextConfigurationOptions("test"));
        var unitOfWorkAccessorNew2 = unitOfWorkNew2.ServiceProvider.GetService<IUnitOfWorkAccessor>();
        Assert.IsTrue(unitOfWorkAccessorNew2!.CurrentDbContextOptions != null &&
            unitOfWorkAccessorNew2.CurrentDbContextOptions.ConnectionString == "test");

        var connectionString =
            await unitOfWorkNew2.ServiceProvider.GetRequiredService<IConnectionStringProvider>().GetConnectionStringAsync();
        Assert.IsTrue(connectionString == "test");
    }

    [TestMethod]
    public void TestUnitOfWorkByEventBusBuilder()
    {
        var services = new ServiceCollection();
        services.Configure<MasaDbConnectionOptions>(options =>
        {
            options.ConnectionStrings = new ConnectionStrings()
            {
                DefaultConnection = _connectionString
            };
        });
        Mock<IEventBusBuilder> eventBuilder = new();
        eventBuilder.Setup(builder => builder.Services).Returns(services).Verifiable();
        eventBuilder.Object.UseUoW<CustomDbContext>(options => options.UseSqlite());

        var serviecProvider = services.BuildServiceProvider();
        Assert.IsNotNull(serviecProvider.GetService<IUnitOfWorkManager>());
        Assert.IsNotNull(serviecProvider.GetService<IUnitOfWorkAccessor>());
        Assert.IsNotNull(serviecProvider.GetService<IUnitOfWork>());
    }

    [TestMethod]
    public void TestUnitOfWorkAndAddMasaConfiguationReturnUnitOfWorkIsNotNull()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddMasaConfiguration();
        Mock<IEventBusBuilder> eventBuilder = new();
        eventBuilder.Setup(eb => eb.Services).Returns(builder.Services).Verifiable();
        eventBuilder.Object.UseUoW<CustomDbContext>(options => options.UseSqlite());

        var serviecProvider = builder.Services.BuildServiceProvider();
        Assert.IsNotNull(serviecProvider.GetService<IUnitOfWorkManager>());
        Assert.IsNotNull(serviecProvider.GetService<IUnitOfWorkAccessor>());
        Assert.IsNotNull(serviecProvider.GetService<IUnitOfWork>());

        var customDbContext = serviecProvider.GetRequiredService<CustomDbContext>();
        Assert.IsTrue(GetDataBaseConnectionString(customDbContext) ==
            serviecProvider.GetRequiredService<IMasaConfiguration>().GetConfiguration(SectionTypes.Local)[
                "ConnectionStrings:DefaultConnection"]);
    }

    [TestMethod]
    public async Task TestGetConnectionStringAndCurrentDbContextOptionsAsyncReturnTest1()
    {
        Mock<IUnitOfWorkAccessor> unitOfWorkAccessor = new();
        string connectionString = "Test1";
        unitOfWorkAccessor.Setup(accessor => accessor.CurrentDbContextOptions)
            .Returns(new MasaDbContextConfigurationOptions(connectionString));
        var connectionStringProvider = new DefaultConnectionStringProvider(unitOfWorkAccessor.Object, null!);
        Assert.IsTrue(await connectionStringProvider.GetConnectionStringAsync() == connectionString);
    }

    [TestMethod]
    public async Task TestGetConnectionStringAsyncReturnTest1()
    {
        Mock<IUnitOfWorkAccessor> unitOfWorkAccessor = new();
        string connectionString = "Test1";
        IServiceCollection services = new ServiceCollection();
        services.Configure<MasaDbConnectionOptions>(options =>
        {
            options.ConnectionStrings = new ConnectionStrings()
            {
                DefaultConnection = connectionString
            };
        });
        var serviceProvider = services.BuildServiceProvider();
        var connectionStringProvider = new DefaultConnectionStringProvider(unitOfWorkAccessor.Object,
            serviceProvider.GetRequiredService<IOptionsMonitor<MasaDbConnectionOptions>>());
        Assert.IsTrue(await connectionStringProvider.GetConnectionStringAsync() == connectionString);
    }

    [TestMethod]
    public async Task TestCommitReturnPublishQueueIsValid()
    {
        IServiceCollection services = new ServiceCollection();
        Mock<IDomainEventBus> domainEventBus = new();
        domainEventBus.Setup(eventBus => eventBus.PublishQueueAsync()).Verifiable();
        services.AddScoped(serviceProvider => domainEventBus.Object);
        Mock<IEventBusBuilder> eventBuilder = new();
        eventBuilder.Setup(eb => eb.Services).Returns(services).Verifiable();
        eventBuilder.Object.UseUoW<CustomDbContext>(options => options.UseTestSqlite($"Data Source=test_{Guid.NewGuid()}"));

        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<CustomDbContext>();
        dbContext.Database.EnsureCreated();
        var unitOfWork = new UnitOfWork<CustomDbContext>(serviceProvider);
        var _ = unitOfWork.Transaction;
        await unitOfWork.CommitAsync();
        domainEventBus.Verify(eventBus => eventBus.PublishQueueAsync(), Times.Once());
    }

    [TestMethod]
    public void SetTemporaryCurrentDbContextOptions()
    {
        var services = new ServiceCollection();
        services.AddScoped<IUnitOfWorkAccessor, UnitOfWorkAccessor>();
        var connectionString = "test1";
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetRequiredService<IUnitOfWorkAccessor>().CurrentDbContextOptions =
            new MasaDbContextConfigurationOptions(connectionString);

        Assert.IsTrue(serviceProvider.GetRequiredService<IUnitOfWorkAccessor>().CurrentDbContextOptions!.ConnectionString ==
            connectionString);

        var connectionString2 = "test2";
        using (serviceProvider.GetRequiredService<IUnitOfWorkAccessor>()
                   .SetTemporaryCurrentDbContextOptions(new MasaDbContextConfigurationOptions(connectionString2)))
        {
            Assert.IsTrue(serviceProvider.GetRequiredService<IUnitOfWorkAccessor>().CurrentDbContextOptions!.ConnectionString ==
                connectionString2);
        }
        Assert.IsTrue(serviceProvider.GetRequiredService<IUnitOfWorkAccessor>().CurrentDbContextOptions!.ConnectionString ==
            connectionString);
    }

    private string GetDataBaseConnectionString(CustomDbContext dbContext) => dbContext.Database.GetConnectionString()!;
}
