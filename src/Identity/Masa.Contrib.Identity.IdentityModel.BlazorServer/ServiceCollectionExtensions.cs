// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Identity.IdentityModel.BlazorServer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMasaIdentityModel(
        this IServiceCollection services,
        IdentityType identityType = IdentityType.Basic)
        => services.AddMasaIdentityModel(identityType, _ =>
        {
        });

    public static IServiceCollection AddMasaIdentityModel(
        this IServiceCollection services,
        IdentityType identityType,
        Action<IdentityClaimOptions> configureOptions)
    {
        services.TryAddScoped<ICurrentPrincipalAccessor, BlazorCurrentPrincipalAccessor>();
        return services.AddMasaIdentityModelCore(identityType, configureOptions);
    }
}