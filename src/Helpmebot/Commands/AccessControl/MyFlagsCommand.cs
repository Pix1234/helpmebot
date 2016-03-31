// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyFlagsCommand.cs" company="Helpmebot Development Team">
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
//   The my flags command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.AccessControl
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model.Interfaces;

    using NHibernate;

    /// <summary>
    /// The my flags command.
    /// </summary>
    [CommandInvocation("myflags")]
    [CommandInvocation("myaccess")]
    [CommandInvocation("whoami")]
    [CommandFlag(Model.Flag.Standard)]
    public class MyFlagsCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="MyFlagsCommand"/> class.
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
        public MyFlagsCommand(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            ILogger logger, 
            ISession databaseSession, 
            ICommandServiceHelper commandServiceHelper)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
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
            var flagsForUser = this.CommandServiceHelper.UserFlagService.GetFlagsForUser(this.User);
            var flagsForChannel = this.CommandServiceHelper.UserFlagService.GetFlagsForChannel(this.CommandChannel).ToList();

            var message = string.Format(
                "The flags currently available to {0} are: {1}",
                this.User,
                flagsForUser.Implode(string.Empty));

            yield return new CommandResponse { Message = message };
            
            if (flagsForChannel.Any())
            {
                message = string.Format(
                    "Additionally, the following flags are gained from {0}: {1}",
                    this.CommandSource,
                    flagsForChannel.Implode(string.Empty));

                yield return new CommandResponse { Message = message };
            }

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
                               string.Empty,
                               new HelpMessage(
                               this.CommandName,
                               string.Empty,
                               "Retrieves the flags available to the current user.")
                           }
                       };
        }

        #endregion
    }
}