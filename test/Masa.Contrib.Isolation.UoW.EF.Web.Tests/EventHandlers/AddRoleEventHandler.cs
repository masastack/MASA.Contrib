// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.Isolation.MultiTenant;

namespace Masa.Contrib.Isolation.UoW.EF.Web.Tests.EventHandlers;

public class AddRoleEventHandler
{
    private readonly CustomDbContext _customDbContext;
    private readonly IDataFilter _dataFilter;
    private readonly IEnvironmentSetter _environmentSetter;
    private readonly IEnvironmentContext _environmentContext;
    private readonly ITenantContext _tenantContext;

    public AddRoleEventHandler(ITenantContext tenantContext,CustomDbContext customDbContext, IDataFilter dataFilter, IEnvironmentSetter environmentSetter,
        IEnvironmentContext environmentContext)
    {
        _tenantContext = tenantContext;
        _customDbContext = customDbContext;
        _dataFilter = dataFilter;
        _environmentSetter = environmentSetter;
        _environmentContext = environmentContext;
    }

    [EventHandler]
    public async Task AddRoleAsync(AddRoleEvent @event)
    {
        await _customDbContext.Database.EnsureCreatedAsync();
        Assert.IsTrue(_customDbContext.Database.GetConnectionString() == "data source=test1");
        var role = new Role
        {
            Name = @event.Name,
            Quantity = @event.Quantity
        };
        await _customDbContext.Set<Role>().AddAsync(role);
        await _customDbContext.SaveChangesAsync();

        var role2 = await _customDbContext.Set<Role>().FirstOrDefaultAsync();
        Assert.IsTrue(role2!.Name == @event.Name);
        Assert.IsTrue(role2.IsDeleted == false);

        _environmentSetter.SetEnvironment("dev"); //In EventHandler, physical isolation is not retriggered if a new DbContext is not recreated, it can only be used to filter changes
        Assert.IsTrue(_environmentContext.CurrentEnvironment == "dev");
        Assert.IsTrue(_customDbContext.Database.GetConnectionString() == "data source=test1");

        var role3 = await _customDbContext.Set<Role>().FirstOrDefaultAsync();
        Assert.IsNull(role3);

        using (_dataFilter.Disable<IMultiEnvironment>())
        {
            Assert.IsTrue(_customDbContext.Database.GetConnectionString() == "data source=test1");
            var role4 = await _customDbContext.Set<Role>().FirstOrDefaultAsync();
            Assert.IsNotNull(role4);
        }
    }
}
