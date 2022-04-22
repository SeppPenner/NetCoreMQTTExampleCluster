// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A base class for all repositories.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;

/// <summary>
/// A base class for all repositories.
/// </summary>
public class BaseRepository
{
    /// <summary>
    /// The connection settings to use.
    /// </summary>
    private readonly MqttDatabaseConnectionSettings connectionSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public BaseRepository(MqttDatabaseConnectionSettings connectionSettings)
    {
        this.connectionSettings = connectionSettings;
        SqlMapper.AddTypeHandler(typeof(PublishedMessagePayload), new JsonMapper<PublishedMessagePayload>());
    }

    /// <summary>
    /// Gets a database connection.
    /// </summary>
    /// <param name="caller">The caller.</param>
    /// <returns>A <see cref="NpgsqlConnection"/>.</returns>
    public async Task<NpgsqlConnection> GetDatabaseConnection([CallerMemberName] string caller = "")
    {
        var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync().ConfigureAwait(false);
        return connection;
    }
}
