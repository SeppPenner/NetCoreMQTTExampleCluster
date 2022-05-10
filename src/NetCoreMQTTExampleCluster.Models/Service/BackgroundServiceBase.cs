// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BackgroundServiceBase.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A base class for all <see cref="BackgroundService"/>s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Service;

/// <summary>
/// A base class for all <see cref="BackgroundService"/>s.
/// </summary>
public abstract class BackgroundServiceBase<T> : BackgroundService where T : class, IConfigurationValid
{
    /// <summary>
    /// The stopwatch for the application lifetime.
    /// </summary>
    protected readonly Stopwatch UptimeStopWatch = Stopwatch.StartNew();

    /// <summary>
    /// The cancellation token source.
    /// </summary>
    protected readonly CancellationTokenSource CancellationTokenSource = new();

    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    protected ILogger Logger { get; set; } = Log.Logger;

    /// <summary>
    /// The last heartbeat timestamp.
    /// </summary>
    protected DateTimeOffset LastHeartbeatAt { get; set; }

    /// <summary>
    /// Gets or sets the Orleans client.
    /// </summary>
    [NotNull]
    protected IClusterClient? OrleansClient { get; set; }

    /// <summary>
    /// The service configuration.
    /// </summary>
    public T ServiceConfiguration { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundServiceBase{T}"/> class.
    /// </summary>
    /// <param name="serviceConfiguration">The service configuration.</param>
    public BackgroundServiceBase(T serviceConfiguration)
    {
        if (!serviceConfiguration.IsValid())
        {
            throw new Exception("The configuration is invalid");
        }

        this.ServiceConfiguration = serviceConfiguration;
    }    

    /// <inheritdoc cref="BackgroundService"/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Connects the Orleans client.
    /// </summary>
    /// <param name="createOrleansClientCallback">The callback to create the Orleans client.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="Exception">The configuration is invalid or the silo connect failed.</exception>
    protected async Task ConnectOrleans(Func<Task> createOrleansClientCallback, CancellationToken cancellationToken)
    {
        await createOrleansClientCallback();

        this.Logger.Information("Connecting to Orleans Silo.");

        if (Debugger.IsAttached)
        {
            await Task.Delay(3000, cancellationToken);
        }

        var isConnected = await this.ConnectOrleansClient(createOrleansClientCallback, cancellationToken);

        if (!isConnected)
        {
            this.Logger.Fatal("Silo is down, terminating.");
            throw new Exception("Silo connect failed.");
        }

        this.Logger.Information("Orleans connected.");
    }

    /// <summary>
    /// Logs the memory information.
    /// </summary>
    /// <param name="heartbeatIntervalInMilliSeconds">The heartbeat interval in milliseconds.</param>
    /// <param name="serviceName">The service name.</param>
    protected void LogMemoryInformation(int heartbeatIntervalInMilliSeconds, string serviceName)
    {
        // Log memory information if the heartbeat is expired.
        if (this.LastHeartbeatAt.IsExpired(TimeSpan.FromMilliseconds(heartbeatIntervalInMilliSeconds)))
        {
            // Run the heartbeat and log some memory information.
            this.LogMemoryInformation(serviceName);
            this.LastHeartbeatAt = DateTimeOffset.Now;
        }
    }

    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <param name="stopMessage">The stop message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected async Task StopService(string stopMessage, CancellationToken cancellationToken)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.CancellationTokenSource.Token, cancellationToken);
        linkedTokenSource?.Cancel();

        try
        {
            await this.StopOrleansClient(cancellationToken);
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            this.Logger.Fatal("An error occurred: {Exception}.", ex);
        }
        finally
        {
            this.Logger.Information(stopMessage);
        }
    }

    /// <summary>
    /// Connects the Orleans client.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="createOrleansClientCallback">The create Orleans client callback.</param>
    private async Task<bool> ConnectOrleansClient(Func<Task> createOrleansClientCallback, CancellationToken cancellationToken)
    {
        await this.OrleansClient.DisposeAsync();

        await createOrleansClientCallback();

        var connectTask = this.OrleansClient.Connect();

        // Unfortunately, this timeout check is required.
        var delay = Task.Delay(6000, cancellationToken);
        var result = await Task.WhenAny(connectTask, delay);

        return result != delay && !result.IsFaulted;
    }

    /// <summary>
    /// Stops the Orleans client.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task<bool> StopOrleansClient(CancellationToken cancellationToken)
    {
        if (this.OrleansClient?.IsInitialized is true)
        {
            _ = this.OrleansClient.Close();
            await Task.Delay(1000, cancellationToken);
            return true;
        }

        await Task.Delay(1000, cancellationToken);
        return false;
    }

    /// <summary>
    /// Logs the memory information.
    /// </summary>
    /// <param name="serviceName">The service name.</param>
    private void LogMemoryInformation(string serviceName)
    {
        var totalMemory = GC.GetTotalMemory(false);
        var memoryInfo = GC.GetGCMemoryInfo();
        var totalMemoryFormatted = FileSizeHelper.GetValueWithUnitByteSize(totalMemory);
        var heapSizeFormatted = FileSizeHelper.GetValueWithUnitByteSize(memoryInfo.HeapSizeBytes);
        var memoryLoadFormatted = FileSizeHelper.GetValueWithUnitByteSize(memoryInfo.MemoryLoadBytes);
        this.Logger.Information(
            "Heartbeat for service {ServiceName}: Total {Total}, heap size: {HeapSize}, memory load: {MemoryLoad}, uptime {Uptime}",
            serviceName, totalMemoryFormatted, heapSizeFormatted, memoryLoadFormatted, this.UptimeStopWatch.Elapsed);
    }
}