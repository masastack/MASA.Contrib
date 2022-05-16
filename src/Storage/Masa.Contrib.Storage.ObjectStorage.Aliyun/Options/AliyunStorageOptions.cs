// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Storage.ObjectStorage.Aliyun.Options;

public class AliyunStorageOptions : AliyunOptions
{
    private string _temporaryCredentialsCacheKey = Const.TEMPORARY_CREDENTIALS_CACHEKEY;

    public string TemporaryCredentialsCacheKey
    {
        get => _temporaryCredentialsCacheKey;
        set => _temporaryCredentialsCacheKey = ObjectStorageExtensions.CheckNullOrEmptyAndReturnValue(value, nameof(TemporaryCredentialsCacheKey));
    }

    /// <summary>
    /// If policy is empty, the user will get all permissions under this role
    /// </summary>
    public string Policy { get; set; }

    public string RoleArn { get; set; }

    public string RoleSessionName { get; set; }

    /// <summary>
    /// The server address of the callback request
    /// </summary>
    public string CallbackUrl { get; set; }

    /// <summary>
    /// The value of the request body when the callback is initiated
    /// </summary>
    public string CallbackBody { get; set; }

    /// <summary>
    /// Large files enable resume after power failure
    /// default: true
    /// </summary>
    public bool EnableResumableUpload { get; set; }

    /// <summary>
    /// large file length
    /// unit: Byte
    /// default: 5GB
    /// </summary>
    public long BigObjectContentLength { get; set; }

    /// <summary>
    /// Gets or sets the size of the part.
    /// </summary>
    /// <value>The size of the part.</value>
    public long? PartSize { get; set; }

    /// <summary>
    /// true: quiet mode; false: detail mode
    /// default: true
    /// </summary>
    public bool Quiet { get; set; }

    public AliyunStorageOptions()
    {
        Quiet = true;
        CallbackUrl = string.Empty;
        CallbackBody = "bucket=${bucket}&object=${object}&etag=${etag}&size=${size}&mimeType=${mimeType}";
        EnableResumableUpload = true;
        PartSize = null;
        BigObjectContentLength = 5 * (long)Math.Pow(1024, 3);
    }

    private AliyunStorageOptions(string accessKeyId, string accessKeySecret) : this()
    {
        AccessKeyId = ObjectStorageExtensions.CheckNullOrEmptyAndReturnValue(accessKeyId, nameof(accessKeyId));
        AccessKeySecret = ObjectStorageExtensions.CheckNullOrEmptyAndReturnValue(accessKeySecret, nameof(accessKeySecret));
    }

    public AliyunStorageOptions(string accessKeyId, string accessKeySecret, string endpoint)
        : this(accessKeyId, accessKeySecret)
    {
        RegionId = GetRegionId(endpoint);
        Endpoint = ObjectStorageExtensions.CheckNullOrEmptyAndReturnValue(endpoint, nameof(endpoint));
    }

    public AliyunStorageOptions(string accessKeyId, string accessKeySecret, string endpoint, string roleArn, string roleSessionName)
        : this(accessKeyId, accessKeySecret, GetRegionId(endpoint), endpoint, roleArn, roleSessionName)
    {
    }

    public AliyunStorageOptions(string accessKeyId, string accessKeySecret, string regionId, string endpoint)
         : this(accessKeyId, accessKeySecret)
    {
        RegionId = ObjectStorageExtensions.CheckNullOrEmptyAndReturnValue(regionId, nameof(regionId));
        Endpoint = ObjectStorageExtensions.CheckNullOrEmptyAndReturnValue(endpoint, nameof(endpoint));
    }

    public AliyunStorageOptions(string accessKeyId, string accessKeySecret, string regionId, string endpoint, string roleArn,
        string roleSessionName)
        : this(accessKeyId, accessKeySecret, regionId, endpoint)
    {
        RoleArn = ObjectStorageExtensions.CheckNullOrEmptyAndReturnValue(roleArn, nameof(roleArn));
        RoleSessionName = ObjectStorageExtensions.CheckNullOrEmptyAndReturnValue(roleSessionName, nameof(roleSessionName));
    }

    public AliyunStorageOptions SetPolicy(string policy)
    {
        Policy = policy;
        return this;
    }

    public AliyunStorageOptions SetTemporaryCredentialsCacheKey(string temporaryCredentialsCacheKey)
    {
        TemporaryCredentialsCacheKey = temporaryCredentialsCacheKey;
        return this;
    }

    public AliyunStorageOptions SetDurationSeconds(long durationSeconds)
    {
        DurationSeconds = durationSeconds;
        return this;
    }

    public AliyunStorageOptions SetEarlyExpires(int earlyExpires)
    {
        EarlyExpires = earlyExpires;
        return this;
    }
}
