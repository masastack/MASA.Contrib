// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Isolation.MultiTenant;

public class TenantContext : ITenantContext, ITenantSetter
{
    private readonly TenantState _tenantState = new();
    private readonly AsyncLocal<TenantState> _state = new();

    public Tenant? CurrentTenant
    {
        get
        {
            _state.Value ??= _tenantState;
            return _state.Value.Tenant;
        }
    }

    public void SetTenant(Tenant? tenant)
    {
        _tenantState.Tenant = tenant;
        if (_state.Value != null)
        {
            _state.Value.Tenant = tenant;
            return;
        }

        _state.Value = new TenantState(tenant);
    }

    public IDisposable SetTemporaryTenant(Tenant? tenant)
    {
        var oldTenant = CurrentTenant;
        SetTenant(tenant);
        return new DisposeAction(() => SetTenant(oldTenant));
    }
}
