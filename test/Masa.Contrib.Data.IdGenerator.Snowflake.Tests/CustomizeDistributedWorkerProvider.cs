﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Masa.Contrib.Data.IdGenerator.Snowflake.Tests;

public class CustomizeDistributedWorkerProvider : DistributedWorkerProvider
{
    public CustomizeDistributedWorkerProvider(DistributedIdGeneratorOptions? distributedIdGeneratorOptions,
        IOptions<RedisConfigurationOptions> redisOptions, ILogger<DistributedWorkerProvider>? logger)
        : base(distributedIdGeneratorOptions, redisOptions, logger)
    {
    }

    protected override async Task<long?> GetWorkerIdByLogOutAsync()
    {
        return null;
    }
}