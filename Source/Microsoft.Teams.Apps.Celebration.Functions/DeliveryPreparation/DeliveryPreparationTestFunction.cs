// <copyright file="DeliveryPreparationTestFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.DeliveryPreparation
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public class DeliveryPreparationTestFunction
    {
        private readonly DeliveryPreparationExecutor deliveryPreparationExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryPreparationTestFunction"/> class.
        /// </summary>
        /// <param name="deliveryPreparationExecutor">The executor for delivery preparation.</param>
        public DeliveryPreparationTestFunction(DeliveryPreparationExecutor deliveryPreparationExecutor)
        {
            this.deliveryPreparationExecutor = deliveryPreparationExecutor;
        }

        [FunctionName("test-dp")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function testing Delivery Preparation.");

            await this.deliveryPreparationExecutor.ExecuteAsync(log);

            return await Task.FromResult((ActionResult)new OkObjectResult("OK"));
        }
    }
}
