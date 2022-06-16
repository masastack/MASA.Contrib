// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Isolation.MultiTenant.Tests;

[TestClass]
public class TestEnvironment
{
    [TestMethod]
    public void TestSetTenant()
    {
        var services = new ServiceCollection();
        Mock<IIsolationBuilder> isolationBuilder = new();
        isolationBuilder.Setup(builder => builder.Services).Returns(services).Verifiable();
        isolationBuilder.Object.UseMultiTenant();

        var serviceProvider = services.BuildServiceProvider();
        Assert.IsTrue(serviceProvider.GetRequiredService<ITenantContext>().CurrentTenant == null);

        var tenant = new Tenant("1");
        serviceProvider.GetRequiredService<ITenantSetter>().SetTenant(tenant);
        Assert.IsTrue(serviceProvider.GetRequiredService<ITenantContext>().CurrentTenant == tenant);
    }

    [TestMethod]
    public void TestSetTemporaryTenant()
    {
        var services = new ServiceCollection();
        Mock<IIsolationBuilder> isolationBuilder = new();
        isolationBuilder.Setup(builder => builder.Services).Returns(services).Verifiable();
        isolationBuilder.Object.UseMultiTenant();

        var serviceProvider = services.BuildServiceProvider();
        Assert.IsTrue(serviceProvider.GetRequiredService<ITenantContext>().CurrentTenant == null);

        var tenant = new Tenant("1");
        serviceProvider.GetRequiredService<ITenantSetter>().SetTenant(tenant);
        Assert.IsTrue(serviceProvider.GetRequiredService<ITenantContext>().CurrentTenant == tenant);

        var tenant2 = new Tenant("2");
        using (serviceProvider.GetRequiredService<ITenantSetter>().SetTemporaryTenant(tenant2))
        {
            var currentTenant = serviceProvider.GetRequiredService<ITenantContext>().CurrentTenant;
            Assert.IsTrue(currentTenant == tenant2);
            Assert.IsTrue(currentTenant != tenant);
        }
        Assert.IsTrue(serviceProvider.GetRequiredService<ITenantContext>().CurrentTenant == tenant);
    }

    [TestMethod]
    public void TestChangeType()
    {
        var convertProvider = new ConvertProvider();
        object result = convertProvider.ChangeType("1", typeof(int));
        Assert.IsTrue(result.Equals(1));

        var guid = Guid.NewGuid();
        result = convertProvider.ChangeType(guid.ToString(), typeof(Guid));
        Assert.IsTrue(result.Equals(guid));

        var str = "dev";
        result = convertProvider.ChangeType(str, typeof(string));
        Assert.IsTrue(result.Equals(str));

        result = convertProvider.ChangeType("1.1", typeof(decimal));
        Assert.IsTrue(result.Equals((decimal)1.1));

        result = convertProvider.ChangeType("1.2", typeof(float));
        Assert.IsTrue(result.Equals((float)1.2));

        result = convertProvider.ChangeType("1.3", typeof(double));
        Assert.IsTrue(result.Equals(1.3d));

        result = convertProvider.ChangeType("1", typeof(ushort));
        Assert.IsTrue(result.Equals((ushort)1));

        bool isProduction = true;
        result = convertProvider.ChangeType(isProduction.ToString(), typeof(bool));
        Assert.IsTrue(result.Equals(isProduction));
    }
}
