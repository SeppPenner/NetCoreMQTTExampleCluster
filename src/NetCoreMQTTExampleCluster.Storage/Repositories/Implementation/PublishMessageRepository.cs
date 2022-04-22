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
/// <seealso cref="IPublishMessageRepository" />
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
    /// <summary>
    /// Gets a <see cref="List{T}"/> of all <see cref="PublishMessage"/>s.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    /// <seealso cref="IPublishMessageRepository" />
    public async Task<List<PublishMessage>> GetPublishMessages()
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var publishMessages = await connection.QueryAsync<PublishMessage>(SelectStatements.SelectAllPublishMessages);
        return publishMessages?.ToList() ?? new List<PublishMessage>();
    }

    /// <inheritdoc cref="IPublishMessageRepository" />
    /// <summary>
    /// Gets a <see cref="PublishMessage" /> by its identifier.
    /// </summary>
    /// <param name="publishMessageId">The <see cref="PublishMessage"/>'s identifier to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    /// <seealso cref="IPublishMessageRepository" />
    public async Task<PublishMessage> GetPublishMessageById(Guid publishMessageId)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<PublishMessage>(SelectStatements.SelectPublishMessageById, new { Id = publishMessageId });
    }

    /// <inheritdoc cref="IPublishMessageRepository" />
    /// <summary>
    /// Inserts a <see cref="PublishMessage" /> to the database.
    /// </summary>
    /// <param name="publishMessage">The <see cref="PublishMessage" /> to insert.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    /// <seealso cref="IPublishMessageRepository" />
    public async Task<bool> InsertPublishMessage(PublishMessage publishMessage)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var result = await connection.ExecuteAsync(InsertStatements.InsertPublishMessage, publishMessage);
        return result == 1;
    }

    /// <inheritdoc cref="IPublishMessageRepository" />
    /// <summary>
    /// Inserts a <see cref="List{T}"/> of <see cref="PublishMessage" />s to the database.
    /// </summary>
    /// <param name="publishMessages">The <see cref="List{T}"/> of <see cref="PublishMessage" />s to insert.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    /// <seealso cref="IPublishMessageRepository" />
    public async Task<bool> InsertPublishMessages(List<PublishMessage> publishMessages)
    {
        await using var connection = new NpgsqlConnection(this.connectionSettings.ToConnectionString());
        await connection.OpenAsync();
        var result = await connection.ExecuteAsync(InsertStatements.InsertPublishMessage, publishMessages);
        return result == 1;
    }
}
