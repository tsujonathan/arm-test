// <copyright file="OccurrenceInitializationExecutor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.OccurrenceInitialization
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.PreviewCard;
    using Microsoft.Teams.Apps.Celebration.Common.Queues;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.User;

    /// <summary>
    /// Creates occurrence 72hours prior to event's due time.
    /// </summary>
    public class OccurrenceInitializationExecutor
    {
        private const string PreviewMessageFormat = "Hi {0}, you have an upcoming event in the next 3 days. If you take no action, I will post this celebration.";
        private readonly EventRepository eventRepository;
        private readonly OccurrenceRepository occurrenceRepository;
        private readonly UserRepository userRepository;
        private readonly SendToConversationQueue sendToConversationQueue;
        private readonly PreviewCardRenderer previewCardRenderer;
        private readonly BotActivityBuilder botActivityBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="OccurrenceInitializationExecutor"/> class.
        /// </summary>
        /// <param name="eventRepository">The event repository service.</param>
        /// <param name="occurrenceRepository">The occurrence repository service.</param>
        /// <param name="userRepository">The user repository service.</param>
        /// <param name="sendToConversationQueue">The SendToConversation message queue service.</param>
        /// <param name="previewCardRenderer">The event occurrence card renderer.</param>
        /// <param name="botActivityBuilder">The bot activity builder.</param>
        public OccurrenceInitializationExecutor(
            EventRepository eventRepository,
            OccurrenceRepository occurrenceRepository,
            UserRepository userRepository,
            SendToConversationQueue sendToConversationQueue,
            PreviewCardRenderer previewCardRenderer,
            BotActivityBuilder botActivityBuilder)
        {
            this.eventRepository = eventRepository;
            this.occurrenceRepository = occurrenceRepository;
            this.userRepository = userRepository;
            this.sendToConversationQueue = sendToConversationQueue;
            this.previewCardRenderer = previewCardRenderer;
            this.botActivityBuilder = botActivityBuilder;
        }

        /// <summary>
        /// Creates occurrence 72 hours prior to event's due time
        /// and sends a message to event owner to notify the occurrence.
        /// </summary>
        /// <param name="log">The logging service.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ExecuteAsync(ILogger log)
        {
            log.LogCritical($"OccurrenceInitializationExecutor executed at: {DateTime.Now}.");

            try
            {
                var eventEntities = await this.eventRepository.GetEventsDueIn72Hours();
                foreach (var eventEntity in eventEntities)
                {
                    var occurrenceEntity = await this.occurrenceRepository.CreateOccurrenceAsync(eventEntity);
                    if (occurrenceEntity == null)
                    {
                        continue;
                    }

                    var userEntity = await this.userRepository.GetAsync(eventEntity.OwnerAadObjectId);
                    if (userEntity == null)
                    {
                        throw new InvalidOperationException($"User {eventEntity.OwnerAadObjectId} not found in repository");
                    }

                    var activity = this.BuildWithPreviewCard(eventEntity, occurrenceEntity, userEntity);
                    await this.sendToConversationQueue.SendActivityAsync(activity);
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{nameof(OccurrenceInitializationExecutor)}{Environment.NewLine}{ex.ToString()}");
            }
        }

        private Activity BuildWithPreviewCard(
            EventEntity eventEntity,
            OccurrenceEntity occurrenceEntity,
            UserEntity userEntity)
        {
            var attachment = this.previewCardRenderer.BuildAttachment(
                eventEntity.Id,
                eventEntity.Title,
                eventEntity.Message,
                eventEntity.Image,
                occurrenceEntity.Id,
                occurrenceEntity.OwnerAadObjectId,
                userEntity.Name);

            var message = string.Format(
                OccurrenceInitializationExecutor.PreviewMessageFormat,
                userEntity.Name);

            var activity = this.botActivityBuilder.CreateActivity(userEntity.ServiceUrl, userEntity.ConversationId);
            activity.Text = message;
            activity.Attachments.Add(attachment);
            return activity;
        }
    }
}