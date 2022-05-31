﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Data.Mapping.Mapster.Internal;

internal class MapsterOptionsExtension : IMapperOptionsExtension
{
    private readonly MapOptions _mapOptions;

    public MapsterOptionsExtension(MapOptions mapOptions) => _mapOptions = mapOptions;

    public void AddService(IServiceCollection services) => services.AddMapping(_mapOptions);
}
