// <copyright file="ITaskModuleService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.TaskModuleServices
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Handles bot task module request.
    /// </summary>
    public interface ITaskModuleService
    {
        /// <summary>
        /// Checks if a task module request can be handled.
        /// </summary>
        /// <param name="taskModuleRequest">The bot task module request.</param>
        /// <returns>The flag indicates if a bot task module can be handled.</returns>
        bool IsApplicable(TaskModuleRequest taskModuleRequest);

        /// <summary>
        /// Handles fetch request.
        /// </summary>
        /// <param name="turnContext">The bot turn context.</param>
        /// <param name="taskModuleRequest">The task module request.</param>
        /// <returns>The task module response.</returns>
        Task<TaskModuleResponse> FetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest);

        /// <summary>
        /// Handles submit request.
        /// </summary>
        /// <param name="turnContext">The bot turn context.</param>
        /// <param name="taskModuleRequest">The task module request.</param>
        /// <returns>The task module response.</returns>
        Task<TaskModuleResponse> SubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest);
    }
}