// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

global using Masa.BuildingBlocks.Data;
global using Masa.BuildingBlocks.Data.Options;
global using Masa.BuildingBlocks.Data.UoW;
global using Masa.BuildingBlocks.Ddd.Domain.Events;
global using Masa.BuildingBlocks.Dispatcher.Events;
global using Masa.Contrib.Data.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using System.Data.Common;
global using System.Text.Json.Serialization;
global using EntityState = Masa.BuildingBlocks.Data.UoW.EntityState;
global using Masa.Contrib.Data.UoW.EF.Internal;
