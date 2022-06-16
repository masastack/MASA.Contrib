// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Isolation.MultiTenant.Internal;

internal class TenantState
{
    public Tenant? Tenant { get; set; }

    public TenantState(Tenant? tenant = null) => Tenant = tenant;
}
