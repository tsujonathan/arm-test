// <copyright file="ChangeMessageTargetCardRenderer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.ChangeMessageTargetCard
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the adaptive card renderer
    /// that renders card for the feature, change message target.
    /// </summary>
    public class ChangeMessageTargetCardRenderer
    {
        /// <summary>
        /// Builds the adaptive card that is used in changing message target.
        /// Return the card as an attachment.
        /// </summary>
        /// <param name="channels">The channels of a team.</param>
        /// <param name="targetChannelId">The id of the message target channel.</param>
        /// <returns>The bot activity attachment with the rendered card.</returns>
        public Attachment BuildAttachment(
            IEnumerable<ChannelInfo> channels,
            string targetChannelId)
        {
            var adaptiveCard = this.Build(channels, targetChannelId);

            return new Attachment
            {
                Content = adaptiveCard,
                ContentType = AdaptiveCard.ContentType,
            };
        }

        /// <summary>
        /// Builds the adaptive card that is used in changing message target.
        /// </summary>
        /// <param name="channels">The channels of a team.</param>
        /// <param name="targetChannelId">The id of the message target channel.</param>
        /// <returns>The rendered card.</returns>
        public AdaptiveCard Build(
            IEnumerable<ChannelInfo> channels,
            string targetChannelId)
        {
            var adaptiveCard = this.GetTaskModuleAdaptiveCard();
            var adaptiveChoiceSetInput = this.GetAdaptiveChoiceSetInput(channels, targetChannelId);
            adaptiveCard.Body.Add(adaptiveChoiceSetInput);
            return adaptiveCard;
        }

        private AdaptiveCard GetTaskModuleAdaptiveCard()
        {
            var taskModuleDTO = new TaskModuleDTO
            {
                TaskModule = TaskModuleConstants.ChangeMessageTargetTaskModuleName,
            };
            var taskModuleDTOAsString = JsonConvert.SerializeObject(taskModuleDTO);

            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock { Text = "Select message target channel:" },
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Submit",
                        DataJson = taskModuleDTOAsString,
                    },
                },
            };

            return card;
        }

        private AdaptiveChoiceSetInput GetAdaptiveChoiceSetInput(
            IEnumerable<ChannelInfo> channels,
            string targetChannelId)
        {
            if (channels == null)
            {
                throw new ArgumentNullException(nameof(channels));
            }

            if (string.IsNullOrWhiteSpace(targetChannelId))
            {
                throw new ArgumentNullException(nameof(targetChannelId));
            }

            var result = new AdaptiveChoiceSetInput
            {
                Id = TaskModuleConstants.ChangeMessageTargetChoiceSetInputId,
                IsMultiSelect = false,
                Choices = new List<AdaptiveChoice>(),
                Style = AdaptiveChoiceInputStyle.Compact,
                Value = targetChannelId,
            };

            foreach (var channel in channels)
            {
                var adaptiveChoice = new AdaptiveChoice
                {
                    Title = string.IsNullOrWhiteSpace(channel.Name) ? "General" : channel.Name,
                    Value = channel.Id,
                };
                result.Choices.Add(adaptiveChoice);
            }

            return result;
        }
    }
}
