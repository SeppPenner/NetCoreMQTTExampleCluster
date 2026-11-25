// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MqttPersistenceQueue.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   Queues the event logs and published messages of all grains in one silo and writes them to the database periodically.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Grains;

/// <inheritdoc cref="IMqttPersistenceQueue" />
/// <summary>
/// Queues the event logs and published messages of all grains in one silo and writes them to the database periodically.
/// </summary>
public sealed class MqttPersistenceQueue : BackgroundService, IMqttPersistenceQueue
{
    /// <summary>
    /// The delay before the queues are written the first time.
    /// </summary>
    private static readonly TimeSpan FirstDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The interval in which the queues are written.
    /// </summary>
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(20);

    /// <summary>
    /// The event log repository.
    /// </summary>
    private readonly IEventLogRepository eventLogRepository;

    /// <summary>
    /// The publish message repository.
    /// </summary>
    private readonly IPublishMessageRepository publishMessageRepository;

    /// <summary>
    /// The event log queue.
    /// </summary>
    private readonly ConcurrentQueue<EventLog> eventLogQueue = new();

    /// <summary>
    /// The publish message queue.
    /// </summary>
    private readonly ConcurrentQueue<PublishMessage> publishMessageQueue = new();

    /// <summary>
    /// The lock that prevents the timer and the shutdown from writing at the same time.
    /// </summary>
    private readonly SemaphoreSlim writeLock = new(1, 1);

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger logger = Log.ForContext<MqttPersistenceQueue>();

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttPersistenceQueue" /> class.
    /// </summary>
    /// <param name="eventLogRepository">The event log repository.</param>
    /// <param name="publishMessageRepository">The publish message repository.</param>
    public MqttPersistenceQueue(IEventLogRepository eventLogRepository, IPublishMessageRepository publishMessageRepository)
    {
        this.eventLogRepository = eventLogRepository;
        this.publishMessageRepository = publishMessageRepository;
    }

    /// <inheritdoc cref="IMqttPersistenceQueue" />
    public void Enqueue(EventLog eventLog)
    {
        this.eventLogQueue.Enqueue(eventLog);
    }

    /// <inheritdoc cref="IMqttPersistenceQueue" />
    public void Enqueue(PublishMessage publishMessage)
    {
        this.publishMessageQueue.Enqueue(publishMessage);
    }

    /// <inheritdoc cref="BackgroundService" />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        // Write what is left, the silo registers this service before itself, so the grains are stopped already.
        await this.WriteQueues();
    }

    /// <inheritdoc cref="BackgroundService" />
    public override void Dispose()
    {
        this.writeLock.Dispose();
        base.Dispose();
    }

    /// <inheritdoc cref="BackgroundService" />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(FirstDelay, stoppingToken);
            using var timer = new PeriodicTimer(Interval);

            do
            {
                await this.WriteQueues();
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The service is stopping, StopAsync writes the rest.
        }
    }

    /// <summary>
    /// Writes the queued event logs and published messages to the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    private async Task WriteQueues()
    {
        await this.writeLock.WaitAsync();

        try
        {
            this.logger.Information(
                "Periodic persisting started, publish message queue size is {PublishMessageQueueCount}, event log queue size is {EventLogQueueCount}.",
                this.publishMessageQueue.Count,
                this.eventLogQueue.Count);
            await this.eventLogRepository.InsertEventLogs(Dequeue(this.eventLogQueue));
            await this.publishMessageRepository.InsertPublishMessages(Dequeue(this.publishMessageQueue));
            this.logger.Information("Periodic persisting finished.");
        }
        catch (Exception ex)
        {
            this.logger.Error("An error occurred: {Exception}.", ex);
        }
        finally
        {
            this.writeLock.Release();
        }
    }

    /// <summary>
    /// Takes all items out of a queue.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="queue">The queue.</param>
    /// <returns>The items in the order they were queued.</returns>
    private static List<T> Dequeue<T>(ConcurrentQueue<T> queue)
    {
        var items = new List<T>();

        while (queue.TryDequeue(out var item))
        {
            items.Add(item);
        }

        return items;
    }
}
