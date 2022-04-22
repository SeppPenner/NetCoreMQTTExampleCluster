// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationException.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An exception to handle configuration errors.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Exceptions;

/// <inheritdoc cref="Exception"/>
/// <summary>
/// An exception to handle configuration errors.
/// </summary>
public class ConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    public ConfigurationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public ConfigurationException(string message) : base(message)
    {
    }
}
