// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishMessageRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An implementation supporting the repository pattern to work with <see cref="PublishMessage" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Implementation;

/// <inheritdoc cref="IPublishMessageRepository" />
/// <summary>
/// An implementation supporting the repository pattern to work with <see cref="PublishMessage" />s.
/// </summary>
public class PublishMessageRepository : IPublishMessageRepository
{
    /// <summary>
    /// The connection settings to use.
    /// </summary>
    private readonly MqttDatabaseConnectionSettings connectionSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishMessageRepository" /> class.
    /// </summary>
    /// <param name="connectionSettings">The connection settings to use.</param>
    public PublishMessageRepository(MqttDatabaseConnectionSettings connectionSettings)
    {
        this.connectionSettings = connectionSettings;
        SqlMapper.AddTypeHandler(typeof(PublishedMessagePayload), new JsonMapper<PublishedMessagePayload>());
    }

    /// <inheritdoc cref="IPublishMessageRepository" />
    public async Task<List<PublishMessage>> GetPublishMessages()
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var publishMessages = await connection.QueryAsync<PublishMessage>(SelectStatements.SelectAllPublishMessages);
        return publishMessages?.ToList() ?? new List<PublishMessage>();
    }

    /// <inheritdoc cref="IPublishMessageRepository" />
    public async Task<PublishMessage> GetPublishMessageById(Guid publishMessageId)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<PublishMessage>(SelectStatements.SelectPublishMessageById, new { Id = publishMessageId });
    }

    /// <inheritdoc cref="IPublishMessageRepository" />
    public async Task<bool> InsertPublishMessage(PublishMessage publishMessage)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var result = await connection.ExecuteAsync(InsertStatements.InsertPublishMessage, publishMessage);
        return result == 1;
    }

    /// <inheritdoc cref="IPublishMessageRepository" />
    public async Task<bool> InsertPublishMessages(List<PublishMessage> publishMessages)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var result = await connection.ExecuteAsync(InsertStatements.InsertPublishMessage, publishMessages);
        return result == 1;
    }
}
