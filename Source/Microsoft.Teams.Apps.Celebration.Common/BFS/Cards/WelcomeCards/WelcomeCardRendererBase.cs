// <copyright file="WelcomeCardRendererBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;

    /// <summary>
    /// This class represents the adaptive card renderer
    /// that renders card contains the shared content of the welcome-related cards.
    /// </summary>
    public class WelcomeCardRendererBase
    {
        private const string GetStartedButtonText = "Get started";
        private const string TakeATourButtonText = "Take a tour";
        private static readonly Uri TourUri = new Uri("https://www.youtube.com");

        private readonly UrlLinkService urlLinkService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeCardRendererBase"/> class.
        /// </summary>
        /// <param name="urlLinkService">The URL link creation service.</param>
        public WelcomeCardRendererBase(UrlLinkService urlLinkService)
        {
            this.urlLinkService = urlLinkService;
        }

        /// <summary>
        /// Renders the adaptive card that contains the shared content of the welcome related cards.
        /// </summary>
        /// <param name="getWelcomeMessageTextBlocks">The callback function to get additional card elements.</param>
        /// <returns>The card rendered by the class.</returns>
        protected AdaptiveCard Render(Func<List<AdaptiveElement>> getWelcomeMessageTextBlocks)
        {
            var welcomeCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = "60",
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveImage()
                                            {
                                                Url = this.urlLinkService.GetCelebrationBotFullColorIcon(),
                                                Size = AdaptiveImageSize.Medium,
                                                Style = AdaptiveImageStyle.Default,
                                            },
                                        },
                                    },
                                    new AdaptiveColumn()
                                    {
                                        Width = "400",
                                        Items = getWelcomeMessageTextBlocks(),
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveOpenUrlAction()
                    {
                        Title = WelcomeCardRendererBase.GetStartedButtonText,
                        Url = this.urlLinkService.GetDeeplinkToEventsTab(),
                    },
                    new AdaptiveOpenUrlAction()
                    {
                        Title = WelcomeCardRendererBase.TakeATourButtonText,
                        Url = WelcomeCardRendererBase.TourUri,
                    },
                },
            };

            return welcomeCard;
        }
    }
}
