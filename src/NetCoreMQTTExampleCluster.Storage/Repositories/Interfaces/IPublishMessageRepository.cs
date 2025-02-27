// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPublishMessageRepository.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface supporting the repository pattern to work with <see cref="PublishMessage" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Repositories.Interfaces;

/// <summary>
/// An interface supporting the repository pattern to work with <see cref="PublishMessage"/>s.
/// </summary>
public interface IPublishMessageRepository
{
    /// <summary>
    /// Gets a <see cref="List{T}"/> of all <see cref="PublishMessage"/>s.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<List<PublishMessage>> GetPublishMessages();

    /// <summary>
    /// Gets a <see cref="PublishMessage" /> by its identifier.
    /// </summary>
    /// <param name="publishMessageId">The <see cref="PublishMessage"/>'s identifier to query for.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<PublishMessage?> GetPublishMessageById(Guid publishMessageId);

    /// <summary>
    /// Inserts a <see cref="PublishMessage" /> to the database.
    /// </summary>
    /// <param name="publishMessage">The <see cref="PublishMessage" /> to insert.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<bool> InsertPublishMessage(PublishMessage publishMessage);

    /// <summary>
    /// Inserts a <see cref="List{T}"/> of <see cref="PublishMessage" />s to the database.
    /// </summary>
    /// <param name="publishMessages">The <see cref="List{T}"/> of <see cref="PublishMessage" />s to insert.</param>
    /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
    Task<bool> InsertPublishMessages(List<PublishMessage> publishMessages);
}
