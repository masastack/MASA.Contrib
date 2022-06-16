// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Data.UoW.EF;

public class UnitOfWorkAccessor : IUnitOfWorkAccessor
{
    private readonly MasaDbContextConfigurationOptionsState _masaDbContextConfigurationOptionsState = new();
    private readonly AsyncLocal<MasaDbContextConfigurationOptionsState> _state = new();

    public MasaDbContextConfigurationOptions? CurrentDbContextOptions
    {
        get
        {
            _state.Value ??= _masaDbContextConfigurationOptionsState;
            return _state.Value.Options;
        }
        set
        {
            _masaDbContextConfigurationOptionsState.Options = value;
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
}
