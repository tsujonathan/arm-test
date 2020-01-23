// <copyright file="BFSClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.RestClient
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the client to the Graph API.
    /// It massages request and response data to ease the usage of Graph API in the installer.
    /// </summary>
    public class BFSClient
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BFSClient"/> class.
        /// </summary>
        /// <param name="httpClient">HTTP client instance.</param>
        public BFSClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Send Bot activity to a conversation.
        /// </summary>
        /// <param name="activity">Bot activity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DeliveryStatus> SendToConversationAsync(Activity activity)
        {
            var url = $"{activity.ServiceUrl}v3/conversations/{activity.Conversation.Id}/activities";

            var messageString = JsonConvert.SerializeObject(activity);

            var payload = new StringContent(messageString, Encoding.UTF8, "application/json");

            using (var response = await this.httpClient.PostAsync(url, payload))
            {
                return this.AnalyseResponse(response);
            }
        }

        /// <summary>
        /// Create a new Bot conversation.
        /// </summary>
        /// <param name="activity">The bot activity instance.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="onConversationCreated">Callback on conversation created.</param>
        /// <returns>The delivery status.</returns>
        public async Task<DeliveryStatus> CreateConversationAsync(
            Activity activity,
            string channelId,
            Action<string> onConversationCreated)
        {
            var url = $"{activity.ServiceUrl}v3/conversations";
            var message = new ConversationParameters
            {
                Activity = activity,
                ChannelData = new TeamsChannelData
                {
                    Channel = new ChannelInfo
                    {
                        Id = channelId,
                    },
                },
            };
            var messageString = JsonConvert.SerializeObject(message);
            var payload = new StringContent(messageString, Encoding.UTF8, "application/json");
            using (var response = await this.httpClient.PostAsync(url, payload))
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseContentAsString = await response.Content.ReadAsStringAsync();
                    var conversationResourceResponse = JsonConvert.DeserializeObject<ConversationResourceResponse>(responseContentAsString);
                    onConversationCreated(conversationResourceResponse.Id);
                }

                return this.AnalyseResponse(response);
            }
        }

        /// <summary>
        /// Update an existing activity.
        /// </summary>
        /// <param name="activity">Bot activity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DeliveryStatus> UpdateActivityAsync(Activity activity)
        {
            var url = $"{activity.ServiceUrl}v3/conversations/{activity.Conversation.Id}/activities/{activity.ReplyToId}";

            var messageString = JsonConvert.SerializeObject(activity);

            var payload = new StringContent(messageString, Encoding.UTF8, "application/json");

            using (var response = await this.httpClient.PutAsync(url, payload))
            {
                return this.AnalyseResponse(response);
            }
        }

        private DeliveryStatus AnalyseResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return DeliveryStatus.Succeeded;
            }
            else if ((int)response.StatusCode == 429)
            {
                return DeliveryStatus.Throttled;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return DeliveryStatus.NotFound;
            }
            else
            {
                return DeliveryStatus.Failed;
            }
        }
    }
}