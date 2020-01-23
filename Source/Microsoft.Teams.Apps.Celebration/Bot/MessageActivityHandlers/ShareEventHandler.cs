// <copyright file="ShareEventHandler.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;
    using Newtonsoft.Json.Linq;

    /// <inheritdoc/>
    /// Handles the message activity, share event.
    public class ShareEventHandler : BaseMessageActivityHandler
    {
        private const string ShareWithTeamSuccessMessage = "I’ve set those events to be shared with the team when they occur.";
        private readonly EventRepository eventRepository;
        private readonly ShareEventCardRenderer shareEventCardRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareEventHandler"/> class.
        /// </summary>
        /// <param name="eventRepository">The event repository.</param>
        /// <param name="shareEventCardRenderer">The share event card renderer.</param>
        public ShareEventHandler(
            EventRepository eventRepository,
            ShareEventCardRenderer shareEventCardRenderer)
        {
            this.eventRepository = eventRepository;
            this.shareEventCardRenderer = shareEventCardRenderer;
        }

        /// <inheritdoc/>
        /// Checks if a message activity is to share an event.
        protected override bool IsApplicable(ITurnContext<IMessageActivity> turnContext)
        {
            return turnContext.Activity.Text.Equals(BotCommandConstants.ShareEvent, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        /// Handles a "share event" message activity.
        protected override async Task HandleAsync(ITurnContext<IMessageActivity> turnContext)
        {
            // Pick up the team id carried in the DTO.
            // Share all the user's events with the team.
            var shareEventCardDTO = (turnContext.Activity.Value as JObject).ToObject<ShareEventCardDTO>();
            var teamId = shareEventCardDTO.TeamId;
            var userAadObjectId = shareEventCardDTO.UserAadObjectId;
            var eventsOwnedByUser = await this.eventRepository.GetAllEventsAsync(userAadObjectId);
            foreach (var eventOwnedByUser in eventsOwnedByUser)
            {
                await this.eventRepository.ShareEventWithTeamAsync(eventOwnedByUser.Id, teamId);
            }

            // Remove the action buttons in the original activity.
            var attachment = this.shareEventCardRenderer.BuildAttachmentWithoutAction(shareEventCardDTO.TeamName);
            var activity = MessageFactory.Attachment(attachment);
            activity.Id = turnContext.Activity.ReplyToId;
            await turnContext.UpdateActivityAsync(activity);

            // Send a message to notify the event sharing.
            activity = MessageFactory.Text(ShareEventHandler.ShareWithTeamSuccessMessage);
            await turnContext.SendActivityAsync(activity);
        }
    }
}