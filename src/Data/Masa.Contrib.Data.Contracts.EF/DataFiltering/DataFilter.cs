// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Data.Contracts.EF.DataFiltering;

public class DataFilter : IDataFilter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MemoryCache<Type, object> _cache;

    public DataFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cache = new();
    }

    public IDisposable Enable<TFilter>() where TFilter : class
        => GetFilter<TFilter>().Enable();

    public IDisposable Disable<TFilter>() where TFilter : class
        => GetFilter<TFilter>().Disable();

    public bool IsEnabled<TFilter>() where TFilter : class
        => GetFilter<TFilter>().Enabled;

    private DataFilter<TFilter> GetFilter<TFilter>()
        where TFilter : class
    {
        return (_cache.GetOrAdd(
            typeof(TFilter),
            _ => _serviceProvider.GetRequiredService<DataFilter<TFilter>>()
        ) as DataFilter<TFilter>)!;
    }
}

public class DataFilter<TFilter> where TFilter : class
{
    private readonly DataFilterState _dataFilterState = new(true);
    private readonly AsyncLocal<DataFilterState> _filter;

    public DataFilter() => _filter = new AsyncLocal<DataFilterState>();

    public bool Enabled
    {
        get
        {
            _filter.Value ??= _dataFilterState;

            return _filter.Value!.Enabled;
        }
    }

    public IDisposable Enable()
    {
        if (Enabled)
            return NullDisposable.Instance;

        SetEnabled(true);
        return new DisposeAction(() => SetEnabled(false));
    }

    public IDisposable Disable()
    {
        if (!Enabled)
            return NullDisposable.Instance;

        SetEnabled(false);
        return new DisposeAction(() => SetEnabled(true));
    }

    private void SetEnabled(bool enabled)
    {
        _dataFilterState.Enabled = enabled;
        _filter.Value!.Enabled = enabled;
    }
}
