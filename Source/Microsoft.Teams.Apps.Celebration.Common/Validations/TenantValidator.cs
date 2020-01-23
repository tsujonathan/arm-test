// <copyright file="TenantValidator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Validations
{
    using System;
    using System.Linq;
    using Microsoft.Teams.Apps.Celebration.Common.Core;

    /// <summary>
    /// Checks if a tenant is authorized.
    /// </summary>
    public class TenantValidator
    {
        private readonly ConfigurationSettings configurationSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantValidator"/> class.
        /// </summary>
        /// <param name="configurationSettings">The configuration settings instance.</param>
        public TenantValidator(ConfigurationSettings configurationSettings)
        {
            this.configurationSettings = configurationSettings;
        }

        /// <summary>
        /// Checks if a tenant is authorized.
        /// </summary>
        /// <param name="tenantId">The id of the tenant to be checked.</param>
        /// <returns>A flag indicate if the tenant is authorized or not.</returns>
        public bool Validate(string tenantId)
        {
            var disableTenantFilter = this.configurationSettings.DisableTenantFilter;
            if (disableTenantFilter)
            {
                return true;
            }

            var allowedTenantIds = this.configurationSettings.AllowedTenants
                ?.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                ?.Select(p => p.Trim());
            if (allowedTenantIds == null || allowedTenantIds.Count() == 0)
            {
                var exceptionMessage = "AllowedTenants setting is not set properly in the configuration file.";
                throw new ApplicationException(exceptionMessage);
            }

            return allowedTenantIds.Contains(tenantId);
        }
    }
}