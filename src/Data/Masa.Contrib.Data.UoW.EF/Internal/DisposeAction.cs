// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Data.UoW.EF.Internal;

internal class DisposeAction : IDisposable
{
    private readonly Action _action;

    public DisposeAction(Action action) => _action = action;

    public void Dispose() => _action.Invoke();
}
