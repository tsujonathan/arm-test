// <copyright file="OccurrenceInitializationTestFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.OccurrenceInitialization
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public class OccurrenceInitializationTestFunction
    {
        private readonly OccurrenceInitializationExecutor occurrenceInitializationExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="OccurrenceInitializationTestFunction"/> class.
        /// </summary>
        /// <param name="occurrenceInitializationExecutor">The executor for occurrence initialization.</param>
        public OccurrenceInitializationTestFunction(
            OccurrenceInitializationExecutor occurrenceInitializationExecutor)
        {
            this.occurrenceInitializationExecutor = occurrenceInitializationExecutor;
        }

        [FunctionName("test-oi")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function testing occurrence initialization.");

            await this.occurrenceInitializationExecutor.ExecuteAsync(log);

            return await Task.FromResult((ActionResult)new OkObjectResult("OK"));
        }
    }
}
