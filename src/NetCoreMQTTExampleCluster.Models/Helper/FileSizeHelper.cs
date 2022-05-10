// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSizeHelper.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A helper class to handle file size information.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Helper;

/// <summary>
/// A helper class to handle file size information.
/// </summary>
public static class FileSizeHelper
{
    /// <summary>
    /// Gets the formatted file size.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="allowedDecimals">The alloed decimals.</param>
    /// <returns>The formatted file size.</returns>
    public static string GetValueWithUnitByteSize(decimal value, int allowedDecimals = 2)
    {
        if (value > GlobalConstants.GigaBytesDivider)
        {
            return $"{Math.Round(value / GlobalConstants.GigaBytesDivider, allowedDecimals)} GB";
        }

        if (value > GlobalConstants.MegaBytesDivider)
        {
            return $"{Math.Round(value / GlobalConstants.MegaBytesDivider, allowedDecimals)} MB";
        }

        if (value > GlobalConstants.KiloBytesDivider)
        {
            return $"{Math.Round(value / GlobalConstants.KiloBytesDivider, allowedDecimals)} kB";
        }

        return $"{value} bytes";
    }
}
