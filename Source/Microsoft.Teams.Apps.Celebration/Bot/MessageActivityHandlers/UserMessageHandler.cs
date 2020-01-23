// <copyright file="UserMessageHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.MessageActivityHandlers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;

    /// <inheritdoc/>
    /// Handles the message activity, user entering message activity.
    public class UserMessageHandler : BaseMessageActivityHandler
    {
        private readonly ResponseToUserMessageCardRenderer welcomeInresponseToUserMessageCardRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessageHandler"/> class.
        /// </summary>
        /// <param name="welcomeInResponseToUserMessageCardRenderer">The in response to user message card renderer.</param>
        public UserMessageHandler(ResponseToUserMessageCardRenderer welcomeInResponseToUserMessageCardRenderer)
        {
            this.welcomeInresponseToUserMessageCardRenderer = welcomeInResponseToUserMessageCardRenderer;
        }

        /// <inheritdoc/>
        /// Checks if a user can handle a message entered by user.
        protected override bool IsApplicable(ITurnContext<IMessageActivity> turnContext)
        {
            var text = turnContext.Activity.Text;
            return !text.Contains(BotCommandConstants.IgnoreEventShare, StringComparison.OrdinalIgnoreCase)
                && !text.Contains(BotCommandConstants.SkipEvent, StringComparison.OrdinalIgnoreCase)
                && !text.Contains(BotCommandConstants.ShareEvent, StringComparison.OrdinalIgnoreCase)
                && !text.Contains(BotCommandConstants.ChangeMessageTarget, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        /// Responses to a message entered by user.
        protected override async Task HandleAsync(ITurnContext<IMessageActivity> turnContext)
        {
            var attachment = this.welcomeInresponseToUserMessageCardRenderer.BuildAttachment();

            var reply = MessageFactory.Attachment(attachment);

            await turnContext.SendActivityAsync(reply);
        }
    }
}