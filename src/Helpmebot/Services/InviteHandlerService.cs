// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InviteHandlerService.cs" company="Helpmebot Development Team">
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
//   The invite handler service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services
{
    using Castle.Core.Logging;

    using Helpmebot.IRC.Events;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The invite handler service.
    /// </summary>
    public class InviteHandlerService : IInviteHandlerService
    {
        #region Fields

        /// <summary>
        /// The command parser.
        /// </summary>
        private readonly ICommandParser commandParser;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="InviteHandlerService"/> class.
        /// </summary>
        /// <param name="commandParser">
        /// The command Parser.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public InviteHandlerService(ICommandParser commandParser, ILogger logger)
        {
            this.commandParser = commandParser;
            this.logger = logger;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on invite received event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public void OnInviteReceivedEvent(object sender, InviteEventArgs e)
        {
            this.logger.InfoFormat("Handling invite event from {0} to channel {1}", e.User, e.Channel);

            this.commandParser.GetCommand(
                new CommandMessage { CommandName = "join", ArgumentList = e.Channel }, 
                e.User, 
                e.Channel, 
                e.Client).Run();
        }

        #endregion
    }
}