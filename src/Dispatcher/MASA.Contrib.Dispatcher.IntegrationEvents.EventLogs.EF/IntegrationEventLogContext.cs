namespace MASA.Contrib.Dispatcher.IntegrationEvents.EventLogs.EF;

public class IntegrationEventLogContext : MasaDbContext
{
    public IntegrationEventLogContext(
        MasaDbContextOptions? options = null,
        MasaDbContextOptions<IntegrationEventLogContext>? eventLogContext = null)
        : base(eventLogContext ?? options ??
            throw new InvalidOperationException("Options extension of type 'CoreOptionsExtension' not found"))
    {
    }

    public DbSet<IntegrationEventLog> EventLogs { get; set; }

    public DbSet<IntegrationEventLogRetryItems> IntegrationEventLogRetryItems { get; set; }

    protected override void OnModelCreatingExecuting(ModelBuilder builder)
    {
        builder.Entity<IntegrationEventLog>(ConfigureEventLogEntry);
        builder.Entity<IntegrationEventLogRetryItems>(ConfigureEventLogRetryItemsEntry);
    }

    private void ConfigureEventLogEntry(EntityTypeBuilder<IntegrationEventLog> builder)
    {
        builder.ToTable("IntegrationEventLog");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.CreationTime)
            .IsRequired();

        builder.Property(e => e.State)
            .IsRequired();

        builder.Property(e => e.TimesSent)
            .IsRequired();

        builder.Property(e => e.EventTypeName)
            .IsRequired();
    }

    private void ConfigureEventLogRetryItemsEntry(EntityTypeBuilder<IntegrationEventLogRetryItems> builder)
    {
        builder.ToTable("IntegrationEventLogRetryItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.LogId)
            .IsRequired();

        builder.Property(e => e.CreationTime)
            .IsRequired();

        builder.Property(e => e.RetryTimes)
            .IsRequired();
    }
}
