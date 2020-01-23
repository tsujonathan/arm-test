// <copyright file="ChangeMessageTargetTaskModuleService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.TaskModuleServices
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.ChangeMessageTargetCard;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <inheritdoc/>
    /// Handles the "change message target" task module request.
    public class ChangeMessageTargetTaskModuleService : ITaskModuleService
    {
        private readonly TeamRepository teamRepository;
        private readonly ChangeMessageTargetCardRenderer changeMessageTargetCardRenderer;
        private readonly MessageTargetChannelNameService messageTargetChannelNameService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeMessageTargetTaskModuleService"/> class.
        /// </summary>
        /// <param name="teamRepository">The team repository.</param>
        /// <param name="changeMessageTargetCardRenderer">The change message target card renderer.</param>
        /// <param name="messageTargetChannelNameService">The message target channel name service.</param>
        public ChangeMessageTargetTaskModuleService(
            TeamRepository teamRepository,
            ChangeMessageTargetCardRenderer changeMessageTargetCardRenderer,
            MessageTargetChannelNameService messageTargetChannelNameService)
        {
            this.teamRepository = teamRepository;
            this.changeMessageTargetCardRenderer = changeMessageTargetCardRenderer;
            this.messageTargetChannelNameService = messageTargetChannelNameService;
        }

        /// <inheritdoc/>
        /// Checks if the task module request is for changing message target.
        public bool IsApplicable(TaskModuleRequest taskModuleRequest)
        {
            var taskModuleDTO = JsonConvert.DeserializeObject<TaskModuleDTO>(taskModuleRequest.Data.ToString());
            return TaskModuleConstants.ChangeMessageTargetTaskModuleName.Equals(taskModuleDTO.TaskModule, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        /// Handles the fetch method on a change message target request.
        public async Task<TaskModuleResponse> FetchAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest)
        {
            var channels = await TeamsInfo.GetTeamChannelsAsync(turnContext);

            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var teamEntity = await this.teamRepository.GetAsync(teamsChannelData?.Team?.Id);
            var messageTargetChannelId = teamEntity?.MessageTargetChannel;

            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Title = "Setup message target channel",
                        Width = 400,
                        Card = this.changeMessageTargetCardRenderer.BuildAttachment(channels, messageTargetChannelId),
                    },
                },
            };
        }

        /// <inheritdoc/>
        /// Handles the submit method on a change message target request.
        public async Task<TaskModuleResponse> SubmitAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest)
        {
            var teamEntity = await this.GetTeamEntityAsync(turnContext, taskModuleRequest);

            var channels = await TeamsInfo.GetTeamChannelsAsync(turnContext);

            var messageTargetChannelName =
                await this.messageTargetChannelNameService.GetMessageTargetChannelNameAsync(channels, teamEntity);

            var whoChangedTarget = turnContext.Activity.From.Name;

            var displayMessage = $"{whoChangedTarget} has selected the channel, {messageTargetChannelName}, as the message target.";
            var reply = MessageFactory.Text(displayMessage);
            await turnContext.SendActivityAsync(reply);

            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = displayMessage,
                },
            };
        }

        private async Task<TeamEntity> GetTeamEntityAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest)
        {
            var requestDataAsJObject = JObject.Parse(taskModuleRequest.Data.ToString());
            var selectedChannelId = requestDataAsJObject[TaskModuleConstants.ChangeMessageTargetChoiceSetInputId]?.ToString();

            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            if (teamsChannelData == null || teamsChannelData.Team == null || string.IsNullOrWhiteSpace(teamsChannelData.Team.Id))
            {
                throw new ApplicationException("Failed to get TeamChannelData from turn context.");
            }

            var teamEntity = await this.teamRepository.GetAsync(teamsChannelData.Team.Id);
            if (teamEntity == null)
            {
                throw new ApplicationException($"Failed to find the team {teamsChannelData.Team.Id} in DB");
            }

            teamEntity.ActiveChannelId = string.IsNullOrWhiteSpace(selectedChannelId) ? null : selectedChannelId;
            await this.teamRepository.CreateOrUpdateAsync(teamEntity);

            return teamEntity;
        }
    }
}