// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConfigurationValid.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface to validate all configurations.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Storage.Interfaces;

/// <summary>
/// An interface to validate all configurations.
/// </summary>
public interface IConfigurationValid
{
    /// <summary>
    /// Checks whether the configuration is valid or not.
    /// </summary>
    /// <returns>A value indicating whether the configuration is valid or not.</returns>
    bool IsValid();
}
