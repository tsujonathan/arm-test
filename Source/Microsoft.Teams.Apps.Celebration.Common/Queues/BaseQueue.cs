// <copyright file="BaseQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.Teams.Apps.Celebration.Common.Core;

    /// <summary>
    /// Base Azure service bus queue service.
    /// </summary>
    public class BaseQueue
    {
        private readonly MessageSender messageSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseQueue"/> class.
        /// </summary>
        /// <param name="configurationSettings">ASP.NET Core <see cref="ConfigurationSettings"/> instance.</param>
        /// <param name="queueName">Azure service bus queue's name.</param>
        public BaseQueue(ConfigurationSettings configurationSettings, string queueName)
        {
            var serviceBusConnectionString = configurationSettings.ServiceBusConnection;
            this.messageSender = new MessageSender(serviceBusConnectionString, queueName);
        }

        /// <summary>
        /// Send a message to Azure service bus queue.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        protected async Task SendAsync(Message message)
        {
            await this.messageSender.SendAsync(message);
        }

        /// <summary>
        /// Send a list of messages to Azure service bus queue.
        /// </summary>
        /// <param name="messageBatch">The message batch to be sent to service bus queue.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        protected async Task SendAsync(IEnumerable<Message> messageBatch)
        {
            if (messageBatch.Count() > 100)
            {
                throw new InvalidOperationException("Exceeded maximum Azure service bus message batch size.");
            }

            await this.messageSender.SendAsync(messageBatch.ToList());
        }
    }
}