using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace MASA.Contrib.Dispatcher.IntegrationEvents.EventLogs.EF.Internal;

internal abstract class QueryFilterProvider : IQueryFilterProvider
{
    public abstract LambdaExpression OnExecuting(IMutableEntityType entityType);
}