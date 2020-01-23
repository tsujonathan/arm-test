// <copyright file="BotActivityBuilder.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.Core;

    /// <summary>
    /// Builds bot activity.
    /// </summary>
    public class BotActivityBuilder
    {
        private readonly string botId;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotActivityBuilder"/> class.
        /// </summary>
        /// <param name="configurationSettings">The configuration settings object.</param>
        public BotActivityBuilder(ConfigurationSettings configurationSettings)
        {
            this.botId = configurationSettings.MicrosoftAppId;
        }

        /// <summary>
        /// Builds a bot activity.
        /// </summary>
        /// <param name="serviceUrl">The bot service URL.</param>
        /// <param name="conversationId">The conversation id.</param>
        /// <returns>Bot created bot activity.</returns>
        public Activity CreateActivity(string serviceUrl, string conversationId)
        {
            return new Activity
            {
                Type = "message",
                ServiceUrl = serviceUrl,
                ChannelId = "msteams",
                From = new ChannelAccount
                {
                    Id = $"28:{this.botId}",
                },
                Conversation = new ConversationAccount
                {
                    Id = conversationId,
                },
                Attachments = new List<Attachment>(),
            };
        }
    }
}