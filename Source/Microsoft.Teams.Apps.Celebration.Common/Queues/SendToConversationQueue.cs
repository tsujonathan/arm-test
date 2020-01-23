// <copyright file="SendToConversationQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Queues
{
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// The message queue service connected to the "send-to-conversation" queue in Azure service bus.
    /// </summary>
    public class SendToConversationQueue : BaseQueue
    {
        /// <summary>
        /// The send to conversation queue name.
        /// </summary>
        public const string QueueName = "send-to-conversation";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendToConversationQueue"/> class.
        /// </summary>
        /// <param name="configurationSettings">ASP.NET Core <see cref="ConfigurationSettings"/> instance.</param>
        public SendToConversationQueue(ConfigurationSettings configurationSettings)
            : base(configurationSettings, SendToConversationQueue.QueueName)
        {
        }

        /// <summary>
        /// Send a message to Azure service bus queue.
        /// </summary>
        /// <param name="activity">The activity to be sent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SendActivityAsync(Activity activity)
        {
            var activityAsString = JsonConvert.SerializeObject(activity);
            var serviceBusQueueMessage = new Message(Encoding.UTF8.GetBytes(activityAsString));
            await this.SendAsync(serviceBusQueueMessage);
        }
    }
}
