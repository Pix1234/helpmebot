// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotifyOnJoinCommand.cs" company="Helpmebot Development Team">
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
//   Defines the NotifyOnJoinCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands.Miscellaneous
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    /// <summary>
    /// The notify.
    /// </summary>
    [CommandInvocation("notify")]
    [CommandFlag(Model.Flag.Protected)]
    public class NotifyOnJoinCommand : CommandBase
    {
        /// <summary>
        /// The notify on join service.
        /// </summary>
        private readonly INotifyOnJoinService notifyOnJoinService;

        /// <summary>
        /// Initialises a new instance of the <see cref="NotifyOnJoinCommand"/> class.
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
        /// <param name="notifyOnJoinService">
        /// The notify On Join Service.
        /// </param>
        public NotifyOnJoinCommand(
            string commandSource,
            IUser user,
            IEnumerable<string> arguments,
            ILogger logger,
            ISession databaseSession,
            ICommandServiceHelper commandServiceHelper,
            INotifyOnJoinService notifyOnJoinService)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
            this.notifyOnJoinService = notifyOnJoinService;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            if (!this.Arguments.Any())
            {
                throw new ArgumentCountException(1, this.Arguments.Count());
            }

            this.notifyOnJoinService.AddNotification(this.Arguments.First(), this.User);

            string response = this.CommandServiceHelper.MessageService.RetrieveMessage(
                "confirmNotify",
                this.CommandSource,
                new[] { this.Arguments.First() });

            yield return new CommandResponse { Message = response };
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
                               "notify",
                               new HelpMessage(
                               this.CommandName,
                               "<nickname>",
                               "Sends you a private message when <nickname> joins a channel the bot is in.")
                           }
                       };
        }
    }
}
