﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Identity;

public sealed class DefaultMultiEnvironmentUserContext: UserContext, IMultiEnvironmentUserContext
{
    public string? Environment => GetUser<IdentityMultiEnvironmentUser>()?.Environment;

    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    private readonly IOptionsMonitor<IdentityClaimOptions> _optionsMonitor;

    public DefaultMultiEnvironmentUserContext(
        ITypeConvertProvider typeConvertProvider,
        ICurrentPrincipalAccessor currentPrincipalAccessor,
        IOptionsMonitor<IdentityClaimOptions> optionsMonitor)
        : base(typeConvertProvider)
    {
        _currentPrincipalAccessor = currentPrincipalAccessor;
        _optionsMonitor = optionsMonitor;
    }

    protected override IdentityMultiEnvironmentUser? GetUser()
    {
        var claimsPrincipal = _currentPrincipalAccessor.GetCurrentPrincipal();
        if (claimsPrincipal == null)
            return null;

        var userId = claimsPrincipal.FindClaimValue(_optionsMonitor.CurrentValue.UserId);
        if (userId == null)
            return null;

        return new IdentityMultiEnvironmentUser
        {
            Id = userId,
            UserName = claimsPrincipal.FindClaimValue(_optionsMonitor.CurrentValue.UserName),
            Environment = claimsPrincipal.FindClaimValue(_optionsMonitor.CurrentValue.Environment),
        };
    }
}