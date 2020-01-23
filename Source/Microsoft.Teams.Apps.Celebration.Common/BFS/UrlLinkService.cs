// <copyright file="UrlLinkService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS
{
    using System;
    using Microsoft.Teams.Apps.Celebration.Common.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Creates URLs used by the Teams application.
    /// </summary>
    public class UrlLinkService
    {
        private readonly string baseUrl;
        private readonly string eventImageUrlFormat;
        private readonly string manifestAppId;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlLinkService"/> class.
        /// </summary>
        /// <param name="configurationSettings">The configuration settings instance.</param>
        public UrlLinkService(ConfigurationSettings configurationSettings)
        {
            this.baseUrl = configurationSettings.BaseUrl;
            this.eventImageUrlFormat = $"{configurationSettings.BaseUrl}/images/Carousel/Celebrations-bot-image-{{0}}-.png";
            this.manifestAppId = configurationSettings.ManifestAppId;
        }

        /// <summary>
        /// Gets an event's image Url.
        /// </summary>
        /// <param name="eventImageCode">The event image code. i.e. 1,2,...,9</param>
        /// <returns>The desired image Url.</returns>
        public string GetEventImageUrl(string eventImageCode)
        {
            return string.Format(this.eventImageUrlFormat, eventImageCode);
        }

        /// <summary>
        /// Gets the deep link to event tab.
        /// If the passing in subEntityId is null, then it points to events tab.
        /// Otherwise, it points a spcific event, which triggers UI show the event in task module.
        /// </summary>
        /// <param name="subEntityId">The event id.</param>
        /// <returns>The deep link to the events tab.</returns>
        public Uri GetDeeplinkToEventsTab(string subEntityId = null)
        {
            string context;
            if (!string.IsNullOrEmpty(subEntityId))
            {
                var contextObject = new
                {
                    subEntityId,
                };
                context = "context=" + Uri.EscapeDataString(JsonConvert.SerializeObject(contextObject));
            }
            else
            {
                context = string.Empty;
            }

            return new Uri(string.Format(
                "https://teams.microsoft.com/l/entity/{0}/{1}?{2}",
                this.manifestAppId,
                "EventsTab",
                context));
        }

        /// <summary>
        /// Gets the URL pointing to the Celebrations app bot's full color icon.
        /// </summary>
        /// <returns>The created URL.</returns>
        public Uri GetCelebrationBotFullColorIcon()
        {
            return new Uri($"{this.baseUrl}/images/celebration_bot_full-color.png");
        }
    }
}
