// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionCommand.cs" company="Helpmebot Development Team">
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
//   Returns the current version of the bot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands.BotManagement
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Model.Interfaces;

    using NHibernate;
#if !DEBUG
    using System;
#endif
    
    /// <summary>
    ///   Returns the current version of the bot.
    /// </summary>
    [CommandInvocation("version")]
    [CommandFlag(Model.Flag.Standard)]
    public class VersionCommand : CommandBase
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="VersionCommand"/> class.
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
        public VersionCommand(
            string commandSource,
            IUser user,
            IEnumerable<string> arguments,
            ILogger logger,
            ISession databaseSession,
            ICommandServiceHelper commandServiceHelper)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            System.Version version = Assembly.GetExecutingAssembly().GetName().Version;

#if !DEBUG
            var date = new DateTime(2000, 1, 1, 0, 0, 0);
            date = date.AddDays(version.Build);
            date = date.AddSeconds(version.Revision * 2);
#endif
            var messageArgs = new List<string>
                                  {
                                      version.Major.ToString(CultureInfo.InvariantCulture),
                                      version.Minor.ToString(CultureInfo.InvariantCulture),
#if DEBUG
                                      "*",
                                      "*",
                                      "DEBUG"
#else
                                      version.Build.ToString(CultureInfo.InvariantCulture),
                                      version.Revision.ToString(CultureInfo.InvariantCulture),
                                      date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
#endif
                                  };

            string message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                "CmdVersion",
                this.CommandSource,
                messageArgs);

            yield return new CommandResponse { Message = message };
        }
    }
}