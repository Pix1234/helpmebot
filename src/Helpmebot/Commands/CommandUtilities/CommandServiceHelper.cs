// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandServiceHelper.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   The command service helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.CommandUtilities
{
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///     The command service helper.
    /// </summary>
    public class CommandServiceHelper : ICommandServiceHelper
    {
        #region Fields

        /// <summary>
        /// The access log service.
        /// </summary>
        private readonly IAccessLogService accessLogService;

        /// <summary>
        ///     The client.
        /// </summary>
        private readonly IIrcClient client;

        /// <summary>
        ///     The configuration helper.
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        ///     The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        ///     The url shortening service.
        /// </summary>
        private readonly IUrlShorteningService urlShorteningService;

        /// <summary>
        /// The user flag service.
        /// </summary>
        private readonly IUserFlagService userFlagService;

        /// <summary>
        /// The channel repository.
        /// </summary>
        private readonly IChannelRepository channelRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandServiceHelper"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="messageService">
        /// The message service.
        /// </param>
        /// <param name="urlShorteningService">
        /// The url Shortening Service.
        /// </param>
        /// <param name="configurationHelper">
        /// The configuration Helper.
        /// </param>
        /// <param name="userFlagService">
        /// The user Flag Service.
        /// </param>
        /// <param name="accessLogService">
        /// The access Log Service.
        /// </param>
        /// <param name="channelRepository">
        /// The channel Repository.
        /// </param>
        public CommandServiceHelper(
            IIrcClient client, 
            IMessageService messageService, 
            IUrlShorteningService urlShorteningService, 
            IConfigurationHelper configurationHelper, 
            IUserFlagService userFlagService, 
            IAccessLogService accessLogService, 
            IChannelRepository channelRepository)
        {
            this.userFlagService = userFlagService;
            this.accessLogService = accessLogService;
            this.channelRepository = channelRepository;
            this.client = client;
            this.messageService = messageService;
            this.urlShorteningService = urlShorteningService;
            this.configurationHelper = configurationHelper;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the access log service.
        /// </summary>
        public IAccessLogService AccessLogService
        {
            get
            {
                return this.accessLogService;
            }
        }

        /// <summary>
        ///     Gets the client.
        /// </summary>
        public IIrcClient Client
        {
            get
            {
                return this.client;
            }
        }

        /// <summary>
        ///     Gets the configuration helper.
        /// </summary>
        public IConfigurationHelper ConfigurationHelper
        {
            get
            {
                return this.configurationHelper;
            }
        }

        /// <summary>
        ///     Gets the message service.
        /// </summary>
        public IMessageService MessageService
        {
            get
            {
                return this.messageService;
            }
        }

        /// <summary>
        ///     Gets the url shortening service.
        /// </summary>
        public IUrlShorteningService UrlShorteningService
        {
            get
            {
                return this.urlShorteningService;
            }
        }

        /// <summary>
        /// Gets the user flag service.
        /// </summary>
        public IUserFlagService UserFlagService
        {
            get
            {
                return this.userFlagService;
            }
        }

        /// <summary>
        /// Gets the channel repository.
        /// </summary>
        public IChannelRepository ChannelRepository
        {
            get
            {
                return this.channelRepository;
            }
        }

        #endregion
    }
}