// <copyright file="OccurrenceInitializationFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.OccurrenceInitialization
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Azure Function Application triggered by timer daily.
    /// It creates occurrence 72hours prior to event's due time.
    /// </summary>
    public class OccurrenceInitializationFunction
    {
        private readonly OccurrenceInitializationExecutor occurrenceInitializationExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="OccurrenceInitializationFunction"/> class.
        /// </summary>
        /// <param name="occurrenceInitializationExecutor">The occurrence initialization executor.</param>
        public OccurrenceInitializationFunction(
            OccurrenceInitializationExecutor occurrenceInitializationExecutor)
        {
            this.occurrenceInitializationExecutor = occurrenceInitializationExecutor;
        }

        /// <summary>
        /// Azure function triggered by timer daily.
        /// It creates occurrence 72 hours prior to event's due time
        /// and sends a message to event owner to notify the occurrence.
        /// </summary>
        /// <param name="occurrenceInitializationTimer">The TimerInfo object coming from the Azure Functions system.</param>
        /// <param name="log">The logging service.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        [FunctionName("OccurrenceInitializationFunction")]
        public async Task RunAsync(
            [TimerTrigger("0 0 0 * * *")]
            TimerInfo occurrenceInitializationTimer,
            ILogger log)
        {
            log.LogCritical($"OccurrenceInitializationFunction executed at: {DateTime.Now}.");

            await this.occurrenceInitializationExecutor.ExecuteAsync(log);
        }
    }
}