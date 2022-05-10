// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiloHostService.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains the Orleans silo host main service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.SiloHost;

/// <inheritdoc cref="BackgroundServiceBase{T}"/>
public class SiloHostService : BackgroundServiceBase<SiloHostConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiloHostService" /> class.
    /// </summary>
    /// <param name="siloHostConfiguration">The silo host configuration..</param>
    public SiloHostService(SiloHostConfiguration siloHostConfiguration) : base(siloHostConfiguration)
    {
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Starting MQTT silo host service...");
        await base.StartAsync(cancellationToken);
        this.Logger.Information("MQTT silo host service started.");
    }

    /// <inheritdoc cref="BackgroundService"/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.Logger.Information("Stopping MQTT silo host service...");
        await base.StopAsync(cancellationToken);
        this.Logger.Information("MQTT silo host service stopped.");
    }

    /// <inheritdoc cref="BackgroundService"/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            this.LogMemoryInformation(this.ServiceConfiguration.HeartbeatIntervalInMilliSeconds, Program.ServiceName.Name ?? "SiloHost");
            await Task.Delay(this.ServiceConfiguration.HeartbeatIntervalInMilliSeconds, stoppingToken);
        }
    }
}
