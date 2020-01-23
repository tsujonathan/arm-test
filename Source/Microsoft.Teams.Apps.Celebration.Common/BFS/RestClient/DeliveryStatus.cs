// <copyright file="DeliveryStatus.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.RestClient
{
    /// <summary>
    /// Event delivery status.
    /// </summary>
    public enum DeliveryStatus
    {
        /// <summary>
        /// Unknown state.
        /// </summary>
        Unknown,

        /// <summary>
        /// Delivering state.
        /// </summary>
        Delivering,

        /// <summary>
        /// Succeeded state.
        /// </summary>
        Succeeded,

        /// <summary>
        /// Failed state.
        /// </summary>
        Failed,

        /// <summary>
        /// Throttled state.
        /// </summary>
        Throttled,

        /// <summary>
        /// Not found state.
        /// </summary>
        NotFound,
    }
}
