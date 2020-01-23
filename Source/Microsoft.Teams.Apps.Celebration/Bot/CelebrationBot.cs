// <copyright file="CelebrationBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers;
    using Microsoft.Teams.Apps.Celebration.Bot.MessageActivityHandlers;
    using Microsoft.Teams.Apps.Celebration.Bot.TaskModuleServices;

    /// <summary>
    /// The Celebrations Bot.
    /// </summary>
    public class CelebrationBot : TeamsActivityHandler
    {
        private readonly IEnumerable<IConversationUpdateActivityHandler> conversationUpdateActivityHandlers;
        private readonly IEnumerable<IMessageActivityHandler> messageActivityHandlers;
        private readonly IEnumerable<ITaskModuleService> taskModuleServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="CelebrationBot"/> class.
        /// </summary>
        /// <param name="conversationUpdateActivityHandlers">IProcessOnConversationUpdateActivityHandler instance list.</param>
        /// <param name="messageActivityHandlers">IMessageActivityHandler instance list.</param>
        /// <param name="taskModuleServices">ITaskModuleService instance list.</param>
        public CelebrationBot(
            IEnumerable<IConversationUpdateActivityHandler> conversationUpdateActivityHandlers,
            IEnumerable<IMessageActivityHandler> messageActivityHandlers,
            IEnumerable<ITaskModuleService> taskModuleServices)
        {
            this.conversationUpdateActivityHandlers = conversationUpdateActivityHandlers;
            this.messageActivityHandlers = messageActivityHandlers;
            this.taskModuleServices = taskModuleServices;
        }

        /// <inheritdoc/>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await base.OnMessageActivityAsync(turnContext, cancellationToken);

            foreach (var messageActivityHandler in this.messageActivityHandlers)
            {
                await messageActivityHandler.HanldeIfApplicableAsync(turnContext);
            }
        }

        /// <summary>
        /// Invoked when a conversation update activity is received from the channel.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnConversationUpdateActivityAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // base.OnConversationUpdateActivityAsync is useful when it comes to responding to users being added to or removed from the conversation.
            // For example, a bot could respond to a user being added by greeting the user.
            // By default, base.OnConversationUpdateActivityAsync will call <see cref="OnMembersAddedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
            // if any users have been added or <see cref="OnMembersRemovedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
            // if any users have been removed. base.OnConversationUpdateActivityAsync checks the member ID so that it only responds to updates regarding members other than the bot itself.
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);

            foreach (var conversationUpdateActivityHandler in this.conversationUpdateActivityHandlers)
            {
                await conversationUpdateActivityHandler.HandleIfApplicableAsync(turnContext);
            }
        }

        /// <inheritdoc/>
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest,
            CancellationToken cancellationToken)
        {
            foreach (var taskModuleService in this.taskModuleServices)
            {
                if (taskModuleService.IsApplicable(taskModuleRequest))
                {
                    return await taskModuleService.FetchAsync(turnContext, taskModuleRequest);
                }
            }

            throw new ArgumentException("No matching service can be found!");
        }

        /// <inheritdoc/>
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest,
            CancellationToken cancellationToken)
        {
            foreach (var taskModuleService in this.taskModuleServices)
            {
                if (taskModuleService.IsApplicable(taskModuleRequest))
                {
                    return await taskModuleService.SubmitAsync(turnContext, taskModuleRequest);
                }
            }

            throw new ArgumentException("No matching service can be found!");
        }
    }
}