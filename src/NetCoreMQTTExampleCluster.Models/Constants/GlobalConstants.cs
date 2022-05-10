// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalConstants.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class for globally defined constants.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.Models.Constants;

/// <summary>
/// A class for globally defined constants.
/// </summary>
public class GlobalConstants
{
    /// <summary>
    /// The invariant.
    /// </summary>
    public const string Invariant = "Npgsql";

    /// <summary>
    /// The simple message stream provider name.
    /// </summary>
    public const string SimpleMessageStreamProvider = "SMSProvider";

    /// <summary>
    /// The pub-sub store.
    /// </summary>
    public const string PubSubStore = "PubSubStore";

    /// <summary>
    /// The repository grain identifier.
    /// </summary>
    public const int RepositoryGrainId = 0;

    /// <summary>
    /// The gigabytes divider. (Used to convert from bytes to gigabytes).
    /// </summary>
    public const decimal GigaBytesDivider = 1024 * 1024 * 1024;

    /// <summary>
    /// The megabytes divider. (Used to convert from bytes to megabytes).
    /// </summary>
    public const decimal MegaBytesDivider = 1024 * 1024;

    /// <summary>
    /// The kilobytes divider. (Used to convert from bytes to kilobytes).
    /// </summary>
    public const decimal KiloBytesDivider = 1024;
}