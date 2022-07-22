// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Configuration.ConfigurationApi.Dcc.Options;

public abstract class ConfigurationApiMasaConfigurationOptions : MasaConfigurationOptions
{
    /// <summary>
    /// The name of the parent section, if it is empty, it will be mounted under SectionType, otherwise it will be mounted to the specified section under SectionType
    /// </summary>
    [JsonIgnore]
    public sealed override string? ParentSection => AppId;

    public virtual string AppId => StaticConfig.AppId;

    /// <summary>
    /// The section null means same as the class name, else load from the specify section
    /// </summary>
    [JsonIgnore]
    public sealed override string? Section => ObjectName;

    public virtual string? ObjectName { get; }

    /// <summary>
    /// Configuration object name
    /// </summary>
    [JsonIgnore]
    public sealed override SectionTypes SectionType => SectionTypes.ConfigurationApi;
}
