// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessLogService.cs" company="Helpmebot Development Team">
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
//   Defines the AccessLogService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;

    using Castle.Core.Internal;
    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    /// <summary>
    /// The access log service.
    /// </summary>
    public class AccessLogService : IAccessLogService
    {
        #region Fields

        /// <summary>
        /// The database session.
        /// </summary>
        private readonly ISession databaseSession;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The user flag service.
        /// </summary>
        private readonly IUserFlagService userFlagService;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="AccessLogService"/> class.
        /// </summary>
        /// <param name="databaseSession">
        /// The database session.
        /// </param>
        /// <param name="userFlagService">
        /// The user flag service.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public AccessLogService(ISession databaseSession, IUserFlagService userFlagService, ILogger logger)
        {
            this.databaseSession = databaseSession;
            this.userFlagService = userFlagService;
            this.logger = logger;

            // override! we want to use the same database session.
            // TODO: verify this.
            this.userFlagService.DatabaseSession = databaseSession;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The failure.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        public void Failure(IUser user, Type command, IEnumerable<string> arguments, string destination)
        {
            this.logger.InfoFormat("Saving FAILED access log entry for {0} executing {1}", user, command);

            this.SaveAccessLogEntry(
                user, 
                command, 
                arguments, 
                destination, 
                false, 
                this.userFlagService.GetFlagsForUser(user).Implode(), 
                command.GetAttribute<CommandFlagAttribute>().Flag);
        }

        /// <summary>
        /// The success.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        public void Success(IUser user, Type command, IEnumerable<string> arguments, string destination)
        {
            this.logger.InfoFormat("Saving SUCCESSFUL access log entry for {0} executing {1}", user, command);

            this.SaveAccessLogEntry(
                user, 
                command, 
                arguments, 
                destination, 
                true, 
                this.userFlagService.GetFlagsForUser(user).Implode(), 
                command.GetAttribute<CommandFlagAttribute>().Flag);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The save access log entry.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="executionAllowed">
        /// The execution allowed.
        /// </param>
        /// <param name="userFlags">
        /// The user flags.
        /// </param>
        /// <param name="requiredFlag">
        /// The required flag.
        /// </param>
        private void SaveAccessLogEntry(
            IUser user, 
            Type command, 
            IEnumerable<string> arguments, 
            string destination, 
            bool executionAllowed, 
            string userFlags, 
            string requiredFlag)
        {
            var userIdentifier = string.Format("{0}!{1}@{2}", user.Nickname, user.Username, user.Hostname);

            var accessLogEntry = new AccessLogEntry
                                     {
                                         Account = user.Account, 
                                         Arguments = arguments.Implode(), 
                                         Channel = destination, 
                                         Command = command.FullName, 
                                         ExecutionAllowed = executionAllowed, 
                                         RequiredFlag = requiredFlag, 
                                         Timestamp = DateTime.Now, 
                                         UserFlags = userFlags, 
                                         UserIdentifier = userIdentifier
                                     };

            this.databaseSession.Save(accessLogEntry);
        }

        #endregion
    }
}