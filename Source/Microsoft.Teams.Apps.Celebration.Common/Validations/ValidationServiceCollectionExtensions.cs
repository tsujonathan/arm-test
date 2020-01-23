// <copyright file="ValidationServiceCollectionExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Validations
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension class for registering validation service in DI container.
    /// </summary>
    public static class ValidationServiceCollectionExtensions
    {
        /// <summary>
        /// Extension method to register validation services in DI container.
        /// </summary>
        /// <param name="services">IServiceCollection instance.</param>
        public static void AddCelebrationsValidations(this IServiceCollection services)
        {
            services.AddTransient<TenantValidator>();
        }
    }
}