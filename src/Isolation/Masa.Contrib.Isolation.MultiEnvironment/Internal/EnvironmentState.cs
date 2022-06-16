// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Isolation.MultiEnvironment.Internal;

internal class EnvironmentState
{
    public string Environment { get; set; }

    public EnvironmentState(string? environment = null) => Environment = environment ?? string.Empty;
}
