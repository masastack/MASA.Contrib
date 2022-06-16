// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Isolation.MultiEnvironment;

public class EnvironmentContext : IEnvironmentContext, IEnvironmentSetter
{
    private readonly EnvironmentState _environmentState = new();
    private readonly AsyncLocal<EnvironmentState> _state = new();

    public string CurrentEnvironment
    {
        get
        {
            _state.Value ??= _environmentState;
            return _state.Value.Environment;
        }
    }

    public void SetEnvironment(string environment)
    {
        _environmentState.Environment = environment;
        if (_state.Value != null)
        {
            _state.Value.Environment = environment;
            return;
        }

        _state.Value = new EnvironmentState(environment);
    }

    public IDisposable SetTemporaryEnvironment(string environment)
    {
        string oldEnvironment = CurrentEnvironment;
        SetEnvironment(environment);
        return new DisposeAction(() => SetEnvironment(oldEnvironment));
    }
}
