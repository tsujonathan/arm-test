// <copyright file="SkipEventHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.MessageActivityHandlers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.PreviewCard;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.User;
    using Newtonsoft.Json.Linq;

    /// <inheritdoc/>
    /// Handles the message activity, skip an event.
    public class SkipEventHandler : BaseMessageActivityHandler
    {
        private const string EventSkippedMessageFormat = "OK, I'll skip {0} this year and won't share it with your teams.";

        private readonly EventRepository eventRepository;
        private readonly OccurrenceRepository occurrenceRepository;
        private readonly UserRepository userRepository;
        private readonly PreviewCardRenderer previewCardRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkipEventHandler"/> class.
        /// </summary>
        /// <param name="eventRepository">The event repository.</param>
        /// <param name="occurrenceRepository">The occurrence repository.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="previewCardRenderer">The preview card renderer.</param>
        public SkipEventHandler(
            EventRepository eventRepository,
            OccurrenceRepository occurrenceRepository,
            UserRepository userRepository,
            PreviewCardRenderer previewCardRenderer)
        {
            this.eventRepository = eventRepository;
            this.occurrenceRepository = occurrenceRepository;
            this.userRepository = userRepository;
            this.previewCardRenderer = previewCardRenderer;
        }

        /// <inheritdoc/>
        /// Checks if a message activity, skip an event.
        protected override bool IsApplicable(ITurnContext<IMessageActivity> turnContext)
        {
            return turnContext.Activity.Text.Equals(BotCommandConstants.SkipEvent, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        /// Handles a "skip an event" message activity.
        protected override async Task HandleAsync(ITurnContext<IMessageActivity> turnContext)
        {
            var previewCardDTO = (turnContext.Activity.Value as JObject).ToObject<PreviewCardDTO>();
            var eventEntity = await this.eventRepository.GetAsync(previewCardDTO.EventId);
            var userEntity = await this.userRepository.GetAsync(previewCardDTO.OwnerAadObjectId);

            // Update activity removing action buttons.
            var attachment = this.previewCardRenderer.BuildAttachmentWithoutSkipButtonAction(
                eventEntity.Id,
                eventEntity.Title,
                eventEntity.Message,
                eventEntity.Image,
                userEntity.Name);
            var activity = MessageFactory.Attachment(attachment);
            activity.Id = turnContext.Activity.ReplyToId;
            await turnContext.UpdateActivityAsync(activity);

            // Send text to notify user of the event is skipped.
            var message = string.Format(SkipEventHandler.EventSkippedMessageFormat, eventEntity.Title);
            activity = MessageFactory.Text(message);
            await turnContext.SendActivityAsync(activity);

            // Set the occurrence's status to skipped in DB.
            await this.occurrenceRepository.SetOccurrenceStateAsync(previewCardDTO.OccurrenceId, OccurrenceState.Skipped);
        }
    }
}