// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcherCommand.cs" company="Helpmebot Development Team">
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
//   The category watcher configuration command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.CategoryWatcher
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Monitoring;

    using NHibernate;

    /// <summary>
    /// The category watcher configuration command.
    /// </summary>
    [CommandFlag(Model.Flag.Configuration)]
    [CommandInvocation("categorywatcher")]
    public class CategoryWatcherCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CategoryWatcherCommand"/> class.
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
        public CategoryWatcherCommand(
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
            if (this.Arguments.Count() < 2)
            {
                throw new ArgumentCountException(2, this.Arguments.Count());
            }

            var mode = this.Arguments.ElementAt(0).ToLowerInvariant();
            var watcher = this.Arguments.ElementAt(1).ToLowerInvariant();

            if (mode == "delay")
            {
                return this.SetDelay(watcher);
            }

            if (mode == "enable")
            {
                return this.EnableWatcher(watcher);
            }

            if (mode == "disable")
            {
                return this.DisableWatcher(watcher);
            }

            if (mode == "status")
            {
                return this.Status(watcher);
            }

            throw new CommandInvocationException();
        }

        /// <summary>
        /// The help.
        /// </summary>
        /// <returns>
        /// The <see cref="IDictionary{String, HelpMessage}"/>.
        /// </returns>
        protected override IDictionary<string, HelpMessage> Help()
        {
            var dict = new Dictionary<string, HelpMessage>();

            dict.Add(
                "delay", 
                new HelpMessage(
                    this.CommandName, 
                    "delay <WatcherName> [newValue]", 
                    "Gets or sets the delay for category watcher reporting globally."));

            return dict;
        }

        /// <summary>
        /// The disable watcher.
        /// </summary>
        /// <param name="watcher">
        /// The watcher.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        private IEnumerable<CommandResponse> DisableWatcher(string watcher)
        {
            WatcherController.Instance().RemoveWatcherFromChannel(watcher, this.CommandSource);

            yield return
                new CommandResponse { Message = this.CommandServiceHelper.MessageService.Done(this.CommandSource) };
        }

        /// <summary>
        /// The enable watcher.
        /// </summary>
        /// <param name="watcher">
        /// The watcher.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        private IEnumerable<CommandResponse> EnableWatcher(string watcher)
        {
            WatcherController.Instance().AddWatcherToChannel(watcher, this.CommandSource);

            yield return
                new CommandResponse { Message = this.CommandServiceHelper.MessageService.Done(this.CommandSource) };
        }

        /// <summary>
        /// The set delay.
        /// </summary>
        /// <param name="watcher">
        /// The watcher.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        private IEnumerable<CommandResponse> SetDelay(string watcher)
        {
            if (this.Arguments.Count() == 3)
            {
                // set the delay if there's three args.
                int newDelay;
                if (!int.TryParse(this.Arguments.ElementAt(2), out newDelay))
                {
                    throw new CommandErrorException("Unable to parse new delay value");
                }

                WatcherController.Instance().SetDelay(watcher, newDelay, this.CommandSource);

                yield return
                    new CommandResponse
                        {
                            Message = string.Format("Delay for watcher {0} set to {1}", watcher, newDelay)
                        };

                yield break;
            }

            var delay = WatcherController.Instance().GetDelay(watcher);

            yield return
                new CommandResponse
                    {
                        Message =
                            string.Format(
                                "Delay for watcher {0} is currently set to {1}",
                                watcher,
                                delay)
                    };
        }

        /// <summary>
        /// The status.
        /// </summary>
        /// <param name="watcher">
        /// The watcher.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        /// <remarks>
        /// TODO: Rewrite! this isn't clear or concise at all.
        /// </remarks>
        private IEnumerable<CommandResponse> Status(string watcher)
        {
            string[] messageParams =
                {
                    watcher, 
                    WatcherController.Instance().IsWatcherInChannel(this.CommandSource, watcher)
                        ? this.CommandServiceHelper.MessageService.RetrieveMessage(
                            Messages.Enabled, 
                            this.CommandSource, 
                            null)
                        : this.CommandServiceHelper.MessageService.RetrieveMessage(
                            Messages.Disabled, 
                            this.CommandSource, 
                            null), 
                    WatcherController.Instance().GetDelay(watcher).ToString(CultureInfo.InvariantCulture)
                };

            var message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                "keywordStatus",
                this.CommandSource,
                messageParams);

            return new CommandResponse { Message = message }.ToEnumerable();
        }

        #endregion
    }
}