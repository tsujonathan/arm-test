// <copyright file="CelebrationBotFilterMiddleware.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.Middlewares
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.Celebration.Common.Validations;

    /// <summary>
    /// The bot's general filter middle-ware.
    /// </summary>
    public class CelebrationBotFilterMiddleware : IMiddleware
    {
        private const string MsTeamsChannelId = "msteams";

        private readonly TenantValidator tenantValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CelebrationBotFilterMiddleware"/> class.
        /// </summary>
        /// <param name="tenantValidator">ASP.NET Core <see cref="IConfiguration"/> instance.</param>
        public CelebrationBotFilterMiddleware(TenantValidator tenantValidator)
        {
            this.tenantValidator = tenantValidator;
        }

        /// <summary>
        /// Processes an incoming activity.
        /// If the activity's channel id is not "msteams", or its conversation's tenant is not an allowed tenant,
        /// then the middle-ware short circuits the pipeline, and skips the middle-wares and handlers
        /// that are listed after this filter in the pipeline.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of a conversation.</param>
        /// <param name="next">The delegate to call to continue the bot middle-ware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            var isMsTeamsChannel = this.ValidateBotFrameworkChannelId(turnContext);
            if (!isMsTeamsChannel)
            {
                return;
            }

            var tenantId = turnContext?.Activity?.Conversation?.TenantId;
            var isAllowedTenant = this.tenantValidator.Validate(tenantId);
            if (!isAllowedTenant)
            {
                return;
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

        private bool ValidateBotFrameworkChannelId(ITurnContext turnContext)
        {
            return CelebrationBotFilterMiddleware.MsTeamsChannelId.Equals(
                turnContext?.Activity?.ChannelId,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
