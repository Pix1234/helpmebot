﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DieCommand.cs" company="Helpmebot Development Team">
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
//   The die command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands.BotManagement
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Model.Interfaces;

    using NHibernate;

    /// <summary>
    /// The die command.
    /// </summary>
    [CommandInvocation("die")]
    [CommandFlag(Model.Flag.Owner)]
    public class DieCommand : CommandBase
    {
        #region Fields

        /// <summary>
        /// The helpmebot.
        /// </summary>
        private readonly Application helpmebot;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="DieCommand"/> class.
        /// </summary>
        /// <param name="commandSource">
        /// The command source.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        /// <param name="commandServiceHelper">
        /// The command Service Helper.
        /// </param>
        /// <param name="helpmebot">
        /// The helpmebot.
        /// </param>
        public DieCommand(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            ILogger logger, 
            ISession databaseSession, 
            ICommandServiceHelper commandServiceHelper, 
            Application helpmebot)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
            this.helpmebot = helpmebot;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            if (this.Arguments.Contains("@confirm"))
            {
                this.helpmebot.ExitFlag.Set();
                yield break;
            }

            var failedMessage = this.CommandServiceHelper.MessageService.RetrieveMessage(
                "Die-unconfirmed", 
                this.CommandSource, 
                null);

            yield return new CommandResponse { Message = failedMessage };
        }

        /// <summary>
        /// The help.
        /// </summary>
        /// <returns>
        /// The <see cref="IDictionary{String, HelpMessage}"/>.
        /// </returns>
        protected override IDictionary<string, HelpMessage> Help()
        {
            return new Dictionary<string, HelpMessage>
                       {
                           {
                               "die", 
                               new HelpMessage(
                               this.CommandName, 
                               string.Empty, 
                               "Shuts down the bot.")
                           }
                       };
        }

        #endregion
    }
}