// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Isolation.MultiEnvironment.Tests;

[TestClass]
public class TestEnvironment
{
    [TestMethod]
    public void TestSetEnvironment()
    {
        var services = new ServiceCollection();
        Mock<IIsolationBuilder> isolationBuilder = new();
        isolationBuilder.Setup(builder => builder.Services).Returns(services).Verifiable();
        isolationBuilder.Object.UseMultiEnvironment();

        var serviceProvider = services.BuildServiceProvider();
        Assert.IsTrue(string.IsNullOrEmpty(serviceProvider.GetRequiredService<IEnvironmentContext>().CurrentEnvironment));

        serviceProvider.GetRequiredService<IEnvironmentSetter>().SetEnvironment("dev");
        Assert.IsTrue(serviceProvider.GetRequiredService<IEnvironmentContext>().CurrentEnvironment == "dev");
    }

    [TestMethod]
    public void TestSetTemporaryEnvironment()
    {
        var services = new ServiceCollection();
        Mock<IIsolationBuilder> isolationBuilder = new();
        isolationBuilder.Setup(builder => builder.Services).Returns(services).Verifiable();
        isolationBuilder.Object.UseMultiEnvironment();

        var serviceProvider = services.BuildServiceProvider();
        Assert.IsTrue(string.IsNullOrEmpty(serviceProvider.GetRequiredService<IEnvironmentContext>().CurrentEnvironment));

        serviceProvider.GetRequiredService<IEnvironmentSetter>().SetEnvironment("dev");
        Assert.IsTrue(serviceProvider.GetRequiredService<IEnvironmentContext>().CurrentEnvironment == "dev");

        using (serviceProvider.GetRequiredService<IEnvironmentSetter>().SetTemporaryEnvironment("production"))
        {
            var currentEnvironment = serviceProvider.GetRequiredService<IEnvironmentContext>().CurrentEnvironment;
            Assert.IsTrue(currentEnvironment == "production");
        }
        Assert.IsTrue(serviceProvider.GetRequiredService<IEnvironmentContext>().CurrentEnvironment == "dev");
    }

    [TestMethod]
    public void TestUseMultiEnvironment()
    {
        IServiceCollection services = new ServiceCollection();
        Mock<IIsolationBuilder> options = new();
        options.Setup(option => option.Services).Returns(services).Verifiable();
        options.Object.UseMultiEnvironment();

        var serviceProvider = services.BuildServiceProvider();
        Assert.IsTrue(serviceProvider.GetService<IIsolationMiddleware>() != null);
        Assert.IsTrue(serviceProvider.GetServices<IIsolationMiddleware>().Count() == 1);
        Assert.IsTrue(serviceProvider.GetService<EnvironmentContext>() != null);
        Assert.IsTrue(serviceProvider.GetService<IEnvironmentContext>() != null);
        Assert.IsTrue(serviceProvider.GetService<IEnvironmentSetter>() != null);
    }

    [TestMethod]
    public void TestUseMultiEnvironment2()
    {
        IServiceCollection services = new ServiceCollection();
        Mock<IIsolationBuilder> options = new();
        options.Setup(option => option.Services).Returns(services).Verifiable();
        options.Object.UseMultiEnvironment().UseMultiEnvironment();

        var serviceProvider = services.BuildServiceProvider();
        Assert.IsTrue(serviceProvider.GetService<IIsolationMiddleware>() != null);
        Assert.IsTrue(serviceProvider.GetServices<IIsolationMiddleware>().Count() == 1);
        Assert.IsTrue(serviceProvider.GetService<EnvironmentContext>() != null);
        Assert.IsTrue(serviceProvider.GetService<IEnvironmentContext>() != null);
        Assert.IsTrue(serviceProvider.GetService<IEnvironmentSetter>() != null);
    }

    [TestMethod]
    public void TestUseMultiEnvironment3()
    {
        IServiceCollection services = new ServiceCollection();
        Mock<IIsolationBuilder> options = new();
        options.Setup(option => option.Services).Returns(services).Verifiable();
        options.Object.UseMultiEnvironment(new List<IParserProvider>() { });

        var serviceProvider = services.BuildServiceProvider();
        Assert.IsTrue(serviceProvider.GetService<IIsolationMiddleware>() != null);
        Assert.IsTrue(serviceProvider.GetServices<IIsolationMiddleware>().Count() == 1);
        Assert.IsTrue(serviceProvider.GetService<EnvironmentContext>() != null);
        Assert.IsTrue(serviceProvider.GetService<IEnvironmentContext>() != null);
        Assert.IsTrue(serviceProvider.GetService<IEnvironmentSetter>() != null);
    }
}
