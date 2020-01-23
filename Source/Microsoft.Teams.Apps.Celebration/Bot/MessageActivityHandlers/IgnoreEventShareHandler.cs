// <copyright file="IgnoreEventShareHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.MessageActivityHandlers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Queues;
    using Newtonsoft.Json.Linq;

    /// <inheritdoc/>
    /// Checks if a message activity is to ignore event share.
    public class IgnoreEventShareHandler : BaseMessageActivityHandler
    {
        private const string ShareEventIngoredMessage = "OK, if you change your mind you can share the events from the Events tab.";
        private readonly ShareEventCardRenderer shareEventCardRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreEventShareHandler"/> class.
        /// </summary>
        /// <param name="shareEventCardRenderer">The share event card renderer.</param>
        public IgnoreEventShareHandler(
            ShareEventCardRenderer shareEventCardRenderer)
        {
            this.shareEventCardRenderer = shareEventCardRenderer;
        }

        /// <inheritdoc/>
        /// Checks if a message activity is to ignore event share or not.
        protected override bool IsApplicable(ITurnContext<IMessageActivity> turnContext)
        {
            return turnContext.Activity.Text.Equals(BotCommandConstants.IgnoreEventShare, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        /// Handles an "ignore event share" message activity.
        protected override async Task HandleAsync(ITurnContext<IMessageActivity> turnContext)
        {
            // Remove the action buttons in the original activity.
            var shareEventCardDTO = (turnContext.Activity.Value as JObject).ToObject<ShareEventCardDTO>();
            var attachment = this.shareEventCardRenderer.BuildAttachmentWithoutAction(shareEventCardDTO.TeamName);
            var activity = MessageFactory.Attachment(attachment);
            activity.Id = turnContext.Activity.ReplyToId;
            await turnContext.UpdateActivityAsync(activity);

            // Send a message to notify the event is ignored.
            activity = MessageFactory.Text(IgnoreEventShareHandler.ShareEventIngoredMessage);
            await turnContext.SendActivityAsync(activity);
        }
    }
}