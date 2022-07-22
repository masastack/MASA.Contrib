// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Configuration.ConfigurationApi.Dcc;

public class ConfigurationApiManage : ConfigurationApiBase, IConfigurationApiManage
{
    private readonly ICallerProvider _callerProvider;

    public ConfigurationApiManage(
        ICallerProvider callerProvider,
        DccSectionOptions defaultSectionOption,
        List<DccSectionOptions>? expandSectionOptions)
        : base(defaultSectionOption, expandSectionOptions)
    {
        _callerProvider = callerProvider;
    }

    ///<inheritdoc/>
    public async Task InitAsync(string environment, string cluster, string appId, Dictionary<string, string> configObjects)
    {
        var requestUri = $"open-api/releasing/init/{GetEnvironment(environment)}/{GetCluster(cluster)}/{GetAppId(appId)}";
        var result = await _callerProvider.PostAsync(requestUri, configObjects, default);

        // 299 is the status code when throwing a UserFriendlyException in masa.framework
        if ((int)result.StatusCode == 299 || !result.IsSuccessStatusCode)
        {
            var error = await result.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }
    }

    public async Task UpdateAsync(string environment, string cluster, string appId, string configObject, object value)
    {
        var requestUri = $"open-api/releasing/{GetEnvironment(environment)}/{GetCluster(cluster)}/{GetAppId(appId)}/{GetConfigObject(configObject)}";
        var result = await _callerProvider.PutAsync(requestUri, value, default);

        // 299 is the status code when throwing a UserFriendlyException in masa.framework
        if ((int)result.StatusCode == 299 || !result.IsSuccessStatusCode)
        {
            var error = await result.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }
    }
}
