中 | [EN](README.md)

## UoW.EF

用例：

```C#
Install-Package Masa.Contrib.Dispatcher.Events
Install-Package Masa.Contrib.Data.UoW.EF
Install-Package Masa.Contrib.Data.EntityFrameworkCore.SqlServer
```

1. 配置appsettings.json
``` appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;uid=sa;pwd=P@ssw0rd;database=identity"
  }
}
```

2. 使用UoW
```C#
builder.Services.AddEventBus(eventBusBuilder => eventBusBuilder.UseUoW<CustomDbContext>(dbOptions => dbOptions.UseSqlServer()));
```

> 提示：UoW暂不支持多DbContext
