// <copyright file="ConfigurationCredentialProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot
{
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Teams.Apps.Celebration.Common.Core;

    /// <summary>
    /// This class implements ICredentialProvider, which is used by the bot framework to retrieve credential info.
    /// </summary>
    public class ConfigurationCredentialProvider : SimpleCredentialProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationCredentialProvider"/> class.
        /// </summary>
        /// <param name="configurationSettings">ConfigurationSettings instance.</param>
        public ConfigurationCredentialProvider(ConfigurationSettings configurationSettings)
            : base(configurationSettings.MicrosoftAppId, configurationSettings.MicrosoftAppPassword)
        {
        }
    }
}