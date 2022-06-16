[中](README.zh-CN.md) | EN

## UoW.EF

Example：

```C#
Install-Package Masa.Contrib.Dispatcher.Events
Install-Package Masa.Contrib.Data.UoW.EF
Install-Package Masa.Contrib.Data.EntityFrameworkCore.SqlServer
```

1. Configure appsettings.json
``` appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;uid=sa;pwd=P@ssw0rd;database=identity"
  }
}
```

2. Use UoW
```C#
builder.Services.AddEventBus(eventBusBuilder => eventBusBuilder.UseUoW<CustomDbContext>(dbOptions => dbOptions.UseSqlServer()));
```

> Tip: UoW does not support multiple DbContexts