// <copyright file="QueueServiceCollectionExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Queues
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension class for registering Azure service bus queue services in DI container.
    /// </summary>
    public static class QueueServiceCollectionExtensions
    {
        /// <summary>
        /// Extension method to register message queue services in DI container.
        /// </summary>
        /// <param name="services">IServiceCollection instance.</param>
        public static void AddCelebrationsMessageQueue(this IServiceCollection services)
        {
            services.AddTransient<SendToConversationQueue>();
        }
    }
}