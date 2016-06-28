// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcherForceCommand.cs" company="Helpmebot Development Team">
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
//   The category watcher force command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.CategoryWatcher
{
    using System.Collections.Generic;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Model.Interfaces;

    using NHibernate;

    /// <summary>
    /// The category watcher force command.
    /// </summary>
    /// <remarks>
    /// This is registered with the command parser from elsewhere, thus doesn't have an invocation attribute.
    /// </remarks>
    [CommandFlag(Model.Flag.Standard)]
    public class CategoryWatcherForceCommand : CommandBase
    {
        private readonly ICategoryWatcherBackgroundService categoryWatcherBackgroundService;

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CategoryWatcherForceCommand"/> class.
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
        /// <param name="categoryWatcherBackgroundService">
        /// The category watcher background service
        /// </param>
        public CategoryWatcherForceCommand(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            ILogger logger, 
            ISession databaseSession, 
            ICommandServiceHelper commandServiceHelper,
            ICategoryWatcherBackgroundService categoryWatcherBackgroundService)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
            this.categoryWatcherBackgroundService = categoryWatcherBackgroundService;
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
            var commandName = this.CommandMessage.CommandName;

            this.categoryWatcherBackgroundService.TriggerCategoryWatcherUpdate(commandName, this.CommandChannel);
            
            yield break;
        }

        #endregion
    }
}