// <copyright file="EventCardRenderer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.EventCard
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence;

    /// <summary>
    /// This class represents the hero card renderer that renders card
    /// used in delivering event occurrence to target audience.
    /// </summary>
    public class EventCardRenderer
    {
        private const string EventCardTitle = "{0} is celebrating {1} with the team.";
        private readonly UrlLinkService urlLinkService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCardRenderer"/> class.
        /// </summary>
        /// <param name="urlLinkService">URL link creation service.</param>
        public EventCardRenderer(UrlLinkService urlLinkService)
        {
            this.urlLinkService = urlLinkService;
        }

        /// <summary>
        /// Builds the hero card that is used in delivering event occurrence to target audience.
        /// </summary>
        /// <param name="notification">The model object that contains the event's info required in rendering the card.</param>
        /// <returns>The bot activity attachment with the rendered card.</returns>
        public Attachment BuildAttachment(Notification notification)
        {
            var card = this.Render(
                notification.EventOwnerDisplayName,
                notification.EventTitle,
                notification.EventMessage,
                this.urlLinkService.GetEventImageUrl(notification.EventImage));

            var attachment = new Attachment
            {
                ContentType = HeroCard.ContentType,
                Content = card,
            };

            return attachment;
        }

        private HeroCard Render(string ownerDisplayName, string eventTitle, string eventMessage, string eventImageUrl)
        {
            return new HeroCard()
            {
                Title = string.Format(EventCardRenderer.EventCardTitle, ownerDisplayName, eventTitle),
                Text = eventMessage,
                Images = new List<CardImage>() { new CardImage(url: eventImageUrl) },
            };
        }
    }
}
