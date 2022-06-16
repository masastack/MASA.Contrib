// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Isolation.Tests;

public class UnitOfWorkAccessor: IUnitOfWorkAccessor
{
    private readonly AsyncLocal<MasaDbContextConfigurationOptionsState> _state = new();

    public MasaDbContextConfigurationOptions? CurrentDbContextOptions
    {
        get
        {
            _state.Value ??= new MasaDbContextConfigurationOptionsState(null);
            return _state.Value.Options;
        }
        set
        {
            if (_state.Value != null)
            {
                _state.Value.Options = value;
                return;
            }

            _state.Value = new MasaDbContextConfigurationOptionsState(value);
        }
    }

    public IDisposable SetTemporaryCurrentDbContextOptions(MasaDbContextConfigurationOptions options)
    {
        var oldOptions = CurrentDbContextOptions;
        CurrentDbContextOptions = options;
        return new DisposeAction(() => CurrentDbContextOptions = oldOptions);
    }

    internal class DisposeAction : IDisposable
    {
        private readonly Action _action;

        public DisposeAction(Action action) => _action = action;

        public void Dispose() => _action.Invoke();
    }

    internal class MasaDbContextConfigurationOptionsState
    {
        public MasaDbContextConfigurationOptions? Options { get; set; }

        public MasaDbContextConfigurationOptionsState(MasaDbContextConfigurationOptions? options) => Options = options;
    }
}
