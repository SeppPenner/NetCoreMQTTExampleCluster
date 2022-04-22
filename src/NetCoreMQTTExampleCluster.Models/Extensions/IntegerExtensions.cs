// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntegerExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains extension method for the <see cref="int" /> data type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Extensions;

/// <summary>
/// A class that contains extension method for the <see cref="int"/> data type.
/// </summary>
public static class IntegerExtensions
{
    /// <summary>
    /// Checks whether a port is valid.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A value indicating whether the port is valid or not.</returns>
    public static bool IsPortValid(this int value)
    {
        return value switch
        {
            <= 0 => false,
            > 65535 => false,
            _ => true
        };
    }
}
