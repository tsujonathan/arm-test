// <copyright file="OccurrenceState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence
{
    /// <summary>
    /// Represents the event occurrence states.
    /// </summary>
    public enum OccurrenceState
    {
        /// <summary>
        /// Unknown state.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Initial state.
        /// </summary>
        Initial,

        /// <summary>
        /// Occurrence is skipped.
        /// </summary>
        Skipped,

        /// <summary>
        /// Deleted state.
        /// </summary>
        Deleted,

        /// <summary>
        /// Delivering state.
        /// </summary>
        Delivering,

        /// <summary>
        /// Delivered state.
        /// </summary>
        Delivered,
    }
}