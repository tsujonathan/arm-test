// <copyright file="DeliveryPreparationFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.DeliveryPreparation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Azure function application triggered by timer hourly.
    /// It finds the overdue event occurrences in DB and send them to users.
    /// </summary>
    public class DeliveryPreparationFunction
    {
        private readonly DeliveryPreparationExecutor deliveryPreparationExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryPreparationFunction"/> class.
        /// </summary>
        /// <param name="deliveryPreparationExecutor">The delivery preparation executor.</param>
        public DeliveryPreparationFunction(
            DeliveryPreparationExecutor deliveryPreparationExecutor)
        {
            this.deliveryPreparationExecutor = deliveryPreparationExecutor;
        }

        /// <summary>
        /// Azure function that finds overdue event occurrences, creates proactive message,
        /// and enqueues the message in the delivery message queue.
        /// </summary>
        /// <param name="occurrenceInitializationTimer">The TimerInfo object coming from the Azure Functions system.</param>
        /// <param name="log">The logging service.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        [FunctionName("DeliveryPreparationFunction")]
        public async Task RunAsync(
            [TimerTrigger("0 0 * * * *")]
            TimerInfo occurrenceInitializationTimer,
            ILogger log)
        {
            log.LogCritical($"DeliveryPreparationFunction executed at: {DateTime.Now}.");

            await this.deliveryPreparationExecutor.ExecuteAsync(log);
        }
    }
}