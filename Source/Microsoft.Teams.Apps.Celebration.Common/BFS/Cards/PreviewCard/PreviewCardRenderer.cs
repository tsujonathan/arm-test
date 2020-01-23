// <copyright file="PreviewCardRenderer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.PreviewCard
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;

    /// <summary>
    /// This class represents the hero card renderer
    /// that renders card for the feature, preview card.
    /// </summary>
    public class PreviewCardRenderer
    {
        private const string EventPreviewCardHeader = "{0} is celebrating {1} with the team";
        private const string EditButtonTitle = "Edit";
        private const string SkipButtonTitle = "Skip";

        private readonly UrlLinkService urlLinkService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewCardRenderer"/> class.
        /// </summary>
        /// <param name="urlLinkService">URL link creation service.</param>
        public PreviewCardRenderer(UrlLinkService urlLinkService)
        {
            this.urlLinkService = urlLinkService;
        }

        /// <summary>
        /// Builds the hero card that is used in previewing an event occurrence.
        /// When an event is about to happen in 72 hours, the bot asks user to preview the occurrence.
        /// User can skip the event by using the Skip action defined in the card.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="eventTitle">The event title.</param>
        /// <param name="eventMessage">The event message.</param>
        /// <param name="eventImage">The event image.</param>
        /// <param name="occurrenceId">The event's occurrence id.</param>
        /// <param name="ownerAadObjectId">The event owner's AadObjectId.</param>
        /// <param name="ownerDisplayName">The event owner's display name. </param>
        /// <returns>The bot attachment with the rendered card.</returns>
        public Attachment BuildAttachment(
            string eventId,
            string eventTitle,
            string eventMessage,
            string eventImage,
            string occurrenceId,
            string ownerAadObjectId,
            string ownerDisplayName)
        {
            var card = this.Render(
                eventId,
                eventTitle,
                eventMessage,
                eventImage,
                occurrenceId,
                ownerAadObjectId,
                ownerDisplayName);

            var attachment = new Attachment
            {
                ContentType = HeroCard.ContentType,
                Content = card,
            };

            return attachment;
        }

        /// <summary>
        /// Builds the hero card without actions.
        /// It is used when user decides to skip an event occurrence.
        /// The bot shows user with the card confirming the occurrence has been skipped.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="eventTitle">The event title.</param>
        /// <param name="eventMessage">The event message.</param>
        /// <param name="eventImageCode">The event image code. i.e. 1, 2, ..., 9</param>
        /// <param name="ownerDisplayName">The Owner display name.</param>
        /// <returns>The bot attachment with the rendered card.</returns>
        public Attachment BuildAttachmentWithoutSkipButtonAction(
            string eventId,
            string eventTitle,
            string eventMessage,
            string eventImageCode,
            string ownerDisplayName)
        {
            var card = this.RenderWithoutSkipButtonAction(
                eventId,
                eventTitle,
                eventMessage,
                eventImageCode,
                ownerDisplayName);

            var attachment = new Attachment
            {
                ContentType = HeroCard.ContentType,
                Content = card,
            };

            return attachment;
        }

        private HeroCard Render(
            string eventId,
            string eventTitle,
            string eventMessage,
            string eventImageCode,
            string occurrenceId,
            string ownerAadObjectId,
            string ownerDisplayName)
        {
            var previewCard = this.RenderWithoutSkipButtonAction(
                eventId,
                eventTitle,
                eventMessage,
                eventImageCode,
                ownerDisplayName);

            var skipButtonAction = new CardAction()
            {
                Title = PreviewCardRenderer.SkipButtonTitle,
                DisplayText = PreviewCardRenderer.SkipButtonTitle,
                Type = ActionTypes.MessageBack,
                Text = BotCommandConstants.SkipEvent,
                Value = new PreviewCardDTO
                {
                    Action = BotCommandConstants.SkipEvent,
                    EventId = eventId,
                    OccurrenceId = occurrenceId,
                    OwnerAadObjectId = ownerAadObjectId,
                    OwnerName = ownerDisplayName,
                },
            };

            previewCard.Buttons.Insert(0, skipButtonAction);

            return previewCard;
        }

        private HeroCard RenderWithoutSkipButtonAction(
            string eventId,
            string eventTitle,
            string eventMessage,
            string eventImageCode,
            string ownerDisplayName)
        {
            var cardActions = new List<CardAction>()
            {
                new CardAction()
                {
                    Title = PreviewCardRenderer.EditButtonTitle,
                    Type = ActionTypes.OpenUrl,
                    Value = this.urlLinkService.GetDeeplinkToEventsTab(eventId),
                },
            };

            var previewCard = new HeroCard()
            {
                Title = string.Format(PreviewCardRenderer.EventPreviewCardHeader, ownerDisplayName, eventTitle),
                Text = eventMessage,
                Buttons = cardActions,
                Images = new List<CardImage>() { new CardImage(url: this.urlLinkService.GetEventImageUrl(eventImageCode)) },
            };

            return previewCard;
        }
    }
}
