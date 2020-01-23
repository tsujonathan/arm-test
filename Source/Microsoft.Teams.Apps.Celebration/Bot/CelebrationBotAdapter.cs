// <copyright file="CelebrationBotAdapter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot
{
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Teams.Apps.Celebration.Bot.Middlewares;

    /// <summary>
    /// The Celebrations Bot Adapter.
    /// </summary>
    public class CelebrationBotAdapter : BotFrameworkHttpAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CelebrationBotAdapter"/> class.
        /// </summary>
        /// <param name="credentialProvider">Credential provider service instance.</param>
        /// <param name="celebrationBotFilterMiddleware">Teams message filter middle-ware instance.</param>
        public CelebrationBotAdapter(
            ICredentialProvider credentialProvider,
            CelebrationBotFilterMiddleware celebrationBotFilterMiddleware)
            : base(credentialProvider)
        {
            this.Use(celebrationBotFilterMiddleware);
        }
    }
}