// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Data.UoW.EF.Internal;

internal class MasaDbContextConfigurationOptionsState
{
    public MasaDbContextConfigurationOptions? Options { get; set; }

    public MasaDbContextConfigurationOptionsState(MasaDbContextConfigurationOptions? options = null) => Options = options;
}
