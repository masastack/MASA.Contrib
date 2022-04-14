namespace Masa.Contrib.Configuration.Tests;

[TestClass]
public class ConfigurationTest
{
    [TestMethod]
    public void TestDefaultMasaConfiguration()
    {
        var builder = WebApplication.CreateBuilder();
        var masaConfiguration = new DefaultMasaConfiguration(builder.Configuration);
        var localConfiguration = masaConfiguration.GetConfiguration(SectionTypes.Local);
        var configurationApiConfiguration = masaConfiguration.GetConfiguration(SectionTypes.Local);
        Assert.IsTrue(!((IConfigurationSection)localConfiguration).Exists());
        Assert.IsTrue(!((IConfigurationSection)configurationApiConfiguration).Exists());
    }

    [TestMethod]
    public void TestAddMasaConfigurationShouldThrowException()
    {
        var builder = WebApplication.CreateBuilder();
        Assert.ThrowsException<Exception>(() => builder.AddMasaConfiguration());
    }

    [TestMethod]
    public void TestAddJsonFileShouldReturnRabbitMqOptionAndSystemOptionsExist()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddMasaConfiguration(masaConfigurationBuilder => masaConfigurationBuilder.AddJsonFile("rabbitMq.json", optional: false, reloadOnChange: true));

        var masaConfiguration = new DefaultMasaConfiguration(builder.Configuration);
        var localConfiguration = masaConfiguration.GetConfiguration(SectionTypes.Local);
        var configurationApiConfiguration = masaConfiguration.GetConfiguration(SectionTypes.ConfigurationAPI);
        Assert.IsTrue(((IConfigurationSection)localConfiguration).Exists());
        Assert.IsTrue(!((IConfigurationSection)configurationApiConfiguration).Exists());

        var serviceProvider = builder.Services.BuildServiceProvider();
        var rabbitMqOptions = serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>();
        Assert.IsTrue(rabbitMqOptions is { Value.HostName: "localhost", Value.UserName: "admin", Value.Password: "admin", Value.VirtualHost: "/", Value.Port: "5672" });

        var systemOptions = serviceProvider.GetRequiredService<IOptions<SystemOptions>>();
        Assert.IsTrue(systemOptions is { Value.Name: "Masa TEST" });

        var redisOptions = serviceProvider.GetRequiredService<IOptions<RedisOptions>>();
        Assert.IsTrue(redisOptions is { Value.Ip: null, Value.Password: null, Value.Port: 0 });
    }

    [TestMethod]
    public void TestManuallyMappingShouldReturnRedisExist()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddMasaConfiguration(masaConfigurationBuilder =>
        {
            masaConfigurationBuilder
                .AddJsonFile("rabbitMq.json", optional: false, reloadOnChange: true)
                .AddJsonFile("redis.json", optional: false, reloadOnChange: true);

            masaConfigurationBuilder.UseMasaOptions(options =>
            {
                options.MappingLocal<RedisOptions>();
            });
        });

        var masaConfiguration = new DefaultMasaConfiguration(builder.Configuration);
        var localConfiguration = masaConfiguration.GetConfiguration(SectionTypes.Local);
        var configurationApiConfiguration = masaConfiguration.GetConfiguration(SectionTypes.ConfigurationAPI);
        Assert.IsTrue(((IConfigurationSection)localConfiguration).Exists());
        Assert.IsTrue(!((IConfigurationSection)configurationApiConfiguration).Exists());

        var serviceProvider = builder.Services.BuildServiceProvider();
        var rabbitMqOptions = serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>();
        Assert.IsTrue(rabbitMqOptions is
        { Value.HostName: "localhost", Value.UserName: "admin", Value.Password: "admin", Value.VirtualHost: "/", Value.Port: "5672" });

        var systemOptions = serviceProvider.GetRequiredService<IOptions<SystemOptions>>();
        Assert.IsTrue(systemOptions is { Value.Name: "Masa TEST" });

        var redisOptions = serviceProvider.GetRequiredService<IOptions<RedisOptions>>();
        Assert.IsTrue(redisOptions is { Value.Ip: "localhost", Value.Password: "", Value.Port: 6379 });
    }

    [TestMethod]
    public void TestMasaConfigurationBuilderShouldReturnSourceCount3()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("rabbitMq.json", optional: false, reloadOnChange: true)
            .AddJsonFile("redis.json", optional: false, reloadOnChange: true);
        var masaConfigurationBuilder = new MasaConfigurationBuilder(new ServiceCollection(), configurationBuilder);
        Assert.IsTrue(masaConfigurationBuilder.Sources.Count == 2);

        var appsettingConfigurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        Assert.IsTrue(appsettingConfigurationBuilder.Sources.Count == 1);
        masaConfigurationBuilder.Add(appsettingConfigurationBuilder.Sources.FirstOrDefault()!);
        Assert.IsTrue(masaConfigurationBuilder.Sources.Count == 3);

        Assert.IsTrue(masaConfigurationBuilder.Build()["KafkaOptions:Servers"] == appsettingConfigurationBuilder.Build()["KafkaOptions:Servers"]);

        Assert.IsTrue(masaConfigurationBuilder.Properties.Count == configurationBuilder.Properties.Count);
    }

    [TestMethod]
    public void TestAddMultiMasaConfigurationShouldReturnIMasaConfigurationCount1()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddMasaConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddJsonFile("redis.json", true, true)
                                .AddJsonFile("rabbitMq.json", true, true);

            configurationBuilder.UseMasaOptions(option => option.MappingLocal<RedisOptions>("RedisOptions"));
        }).AddMasaConfiguration();
        var serviceProvider = builder.Services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var redisOption = serviceProvider.GetRequiredService<IOptions<RedisOptions>>();
        Assert.IsTrue(configuration["Local:RedisOptions:Ip"] == "localhost");
        Assert.IsTrue(redisOption.Value.Ip == "localhost");

        var rabbitMqOption = serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>();
        Assert.IsTrue(configuration["Local:RabbitMq:UserName"] == "admin");
        Assert.IsTrue(rabbitMqOption.Value.UserName == "admin" && rabbitMqOption.Value.Password == "admin");

        Assert.IsTrue(serviceProvider.GetServices<IMasaConfiguration>().Count() == 1);
    }

    [TestMethod]
    public void TestAutoMapSectionErrorShouldThrowException()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Host.ConfigureAppConfiguration((_, config) => config.Sources.Clear());
        var chainedConfiguration = new ConfigurationBuilder()
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", true, true);
        builder.Configuration.AddConfiguration(chainedConfiguration.Build());

        Assert.ThrowsException<Exception>(() => builder.AddMasaConfiguration(typeof(ConfigurationTest).Assembly, typeof(KafkaOptions).Assembly)
            , $"Check if the mapping section is correct，section name is [{It.IsAny<string>()}]");
    }

    [TestMethod]
    public void TestSpecifyAssembliesShouldKafKaOptionsExist()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Host.ConfigureAppConfiguration((_, config) => config.Sources.Clear());
        var chainedConfiguration = new ConfigurationBuilder()
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", true, true);
        builder.Configuration.AddConfiguration(chainedConfiguration.Build());
        builder.AddMasaConfiguration(typeof(KafkaOptions).Assembly);

        var serviceProvider = builder.Services.BuildServiceProvider();
        var kafkaOptions = serviceProvider.GetRequiredService<IOptions<KafkaOptions>>();
        Assert.IsTrue(kafkaOptions is { Value: { Servers: "Kafka Server", ConnectionPoolSize: 10 } });
    }

    [TestMethod]
    public void TestSpecifyAssembliesShouldThrowException()
    {
        var builder = WebApplication.CreateBuilder();
        Assert.ThrowsException<Exception>(() =>
        {
            return builder.AddMasaConfiguration(configurationBuilder =>
                    configurationBuilder.AddJsonFile("redis.json", true, true),
                typeof(ConfigurationTest).Assembly, typeof(MountSectionRedisOptions).Assembly);
        }, $"Check if the mapping section is correct，section name is [{It.IsAny<string>()}]");
    }

    [TestMethod]
    public void TestNoParameterlessConstructorSpecifyAssembliesShouldThrowException()
    {
        var builder = WebApplication.CreateBuilder();
        Assert.ThrowsException<Exception>(() => builder.AddMasaConfiguration(typeof(ConfigurationTest).Assembly, typeof(EsOptions).Assembly), $"[{It.IsAny<string>()}] must have a parameterless constructor");
    }

    [TestMethod]
    public void TestRepeatMapptingShouldThrowException()
    {
        var builder = WebApplication.CreateBuilder();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            builder.AddMasaConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddJsonFile("redis.json", true, true)
                                    .AddJsonFile("rabbitMq.json", true, true);

                configurationBuilder.UseMasaOptions(option => option.MappingLocal<RedisOptions>().MappingLocal<RedisOptions>());
            });
        }, "The current section already has a configuration");
    }

    [TestMethod]
    public void TestCreateMasaConfigurationShouldReturnRedisOptionsAndSystemOptionsExist()
    {
        var services = new ServiceCollection();
        services.CreateMasaConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddJsonFile("redis.json", true, true)
                .AddJsonFile("rabbitMq.json", true, true);

            configurationBuilder.UseMasaOptions(option => option.MappingLocal<RedisOptions>().MappingLocal<SystemOptions>());
        }, new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true));
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        var redisOption = serviceProvider.GetRequiredService<IOptions<RedisOptions>>();
        Assert.IsTrue(redisOption.Value.Ip == "localhost");

        var systemOptions = serviceProvider.GetRequiredService<IOptions<SystemOptions>>();
        Assert.IsTrue(systemOptions is { Value.Name: "Masa TEST" });
    }

    [TestMethod]
    public async Task TestConfigurationChangeShouldReturnNameEmpty()
    {
        var builder = WebApplication.CreateBuilder();

        var rootPath = builder.Environment.ContentRootPath;
        builder.AddMasaConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddJsonFile("redis.json", true, true)
                .AddJsonFile("rabbitMq.json", true, true);

            configurationBuilder.UseMasaOptions(option => option.MappingLocal<RedisOptions>());
        }, typeof(ConfigurationTest).Assembly);
        var serviceProvider = builder.Services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var systemOption = serviceProvider.GetRequiredService<IOptions<SystemOptions>>();

        Assert.IsNotNull(configuration);
        Assert.IsNotNull(systemOption);
        Assert.IsTrue(systemOption.Value.Name == "Masa TEST");

        var newRedisOption = systemOption.Value;
        newRedisOption.Name = null;

        var oldContent = await File.ReadAllTextAsync(Path.Combine(rootPath, "appsettings.json"));
        File.WriteAllText(Path.Combine(rootPath, "appsettings.json"),
            System.Text.Json.JsonSerializer.Serialize(new { SystemOptions = newRedisOption }));

        Thread.Sleep(2000);
        var option = serviceProvider.GetRequiredService<IOptionsMonitor<SystemOptions>>();
        Assert.IsTrue(option.CurrentValue.Name == "");

        File.WriteAllText(Path.Combine(rootPath, "appsettings.json"), oldContent);
    }
}
