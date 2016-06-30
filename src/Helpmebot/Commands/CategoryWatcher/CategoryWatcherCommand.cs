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
    using System.Linq;

    using Castle.Core.Internal;
    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    using NHibernate;
    using NHibernate.Criterion;

    /// <summary>
    /// The category watcher configuration command.
    /// </summary>
    [CommandFlag(Model.Flag.Configuration)]
    [CommandInvocation("categorywatcher")]
    public class CategoryWatcherCommand : CommandBase
    {
        private readonly ICategoryWatcherBackgroundService categoryWatcherBackgroundService;

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
        /// <param name="categoryWatcherBackgroundService">
        /// The category watcher background service
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
            ICategoryWatcherBackgroundService categoryWatcherBackgroundService, 
            ICommandServiceHelper commandServiceHelper)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
            this.categoryWatcherBackgroundService = categoryWatcherBackgroundService;
        }

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

            if (mode == "add")
            {
                if (this.Arguments.Count() < 3)
                {
                    throw new ArgumentCountException(3, this.Arguments.Count(), "add");
                }

                return this.AddWatcher(watcher, string.Join(" ", this.Arguments.Skip(2)));
            }

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

            if (mode == "flag")
            {
                return this.SetWatcherFlag(watcher);
            }

            if (mode == "set")
            {
                if (this.Arguments.Count() < 3)
                {
                    throw new ArgumentCountException(3, this.Arguments.Count(), "settext");
                }

                return this.SetWatcherConfig(watcher);
            }

            // if (mode == "status")
            // {
            // return this.Status(watcher);
            // }
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

            dict.Add(
                "add", 
                new HelpMessage(
                    this.CommandName, 
                    "add <WatcherName> <category>", 
                    "Adds a new category watcher to this channel for the specified category"));

            dict.Add(
                "enable", 
                new HelpMessage(
                    this.CommandName, 
                    "enable <WatcherName>", 
                    "Enables a category watcher in this channel"));

            dict.Add(
                "disable", 
                new HelpMessage(
                    this.CommandName, 
                    "disable <WatcherName>", 
                    "Disables a category watcher in this channel"));

            dict.Add(
                "flag",
                new HelpMessage(
                    this.CommandName,
                    "flag <WatcherName> [<+|->showwikilink] [<+|->showshorturl] [<+|->showwaittime] [<+|->delta]",
                    "Enables or disables features for this watcher - for example, to enable the short URLs, use:  flag <WatcherName> +shorturls"));

            dict.Add(
                "set",
                new HelpMessage(
                    this.CommandName,
                    "set <WatcherName> <singular|plural|action|category|priority|mintime> <value>",
                    "Sets the text used for reports, the category to watch, the priority, or the minimum wait time before reporting delay of the category watcher"));

            return dict;
        }

        private IEnumerable<CommandResponse> AddWatcher(string watcher, string category)
        {
            var list =
                this.DatabaseSession.CreateCriteria<CategoryWatcher>()
                    .Add(Restrictions.Eq("Channel", this.CommandChannel))
                    .Add(Restrictions.Eq("Keyword", watcher))
                    .List();
            if (list.Count > 0)
            {
                yield return new CommandResponse { Message = "Already exists!" };
                yield break;
            }

            var cw = new CategoryWatcher
                         {
                             Category = category, 
                             Channel = this.CommandChannel, 
                             Enabled = true, 
                             Keyword = watcher, 
                             MediaWikiSite = this.CommandChannel.BaseWiki
                         };

            this.DatabaseSession.SaveOrUpdate(cw);

            this.categoryWatcherBackgroundService.EnableWatcher(cw);

            yield return
                new CommandResponse { Message = this.CommandServiceHelper.MessageService.Done(this.CommandSource) };
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
            var categoryWatcher = this.GetCategoryWatcher(watcher);

            categoryWatcher.Enabled = false;
            this.DatabaseSession.Update(categoryWatcher);

            this.categoryWatcherBackgroundService.DisableWatcher(categoryWatcher);

            yield return
                new CommandResponse { Message = this.CommandServiceHelper.MessageService.Done(this.CommandSource) };

            throw new CommandErrorException("Ambiguous watcher definition");
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
            var categoryWatcher = this.GetCategoryWatcher(watcher);

            categoryWatcher.Enabled = true;
            this.DatabaseSession.Update(categoryWatcher);

            this.categoryWatcherBackgroundService.EnableWatcher(categoryWatcher);

            yield return
                new CommandResponse { Message = this.CommandServiceHelper.MessageService.Done(this.CommandSource) };

            throw new CommandErrorException("Ambiguous watcher definition");
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
            var categoryWatcher = this.GetCategoryWatcher(watcher);

            if (this.Arguments.Count() == 3)
            {
                // set the delay if there's three args.
                int newDelay;
                if (!int.TryParse(this.Arguments.ElementAt(2), out newDelay))
                {
                    throw new CommandErrorException("Unable to parse new delay value");
                }

                this.categoryWatcherBackgroundService.DisableWatcher(categoryWatcher);

                categoryWatcher.SleepTime = newDelay;
                this.DatabaseSession.Update(categoryWatcher);

                this.categoryWatcherBackgroundService.EnableWatcher(categoryWatcher);

                yield return
                    new CommandResponse
                        {
                            Message = string.Format("Delay for watcher {0} set to {1}", watcher, newDelay)
                        };

                yield break;
            }

            var delay = categoryWatcher.SleepTime;

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

        private IEnumerable<CommandResponse> SetWatcherFlag(string watcher)
        {
            var categoryWatcher = this.GetCategoryWatcher(watcher);

            if (this.Arguments.Count() > 2)
            {
                this.Arguments.Skip(2).ForEach(
                    x =>
                        {
                            bool? state = null;

                            if (x.StartsWith("+")) state = true;
                            if (x.StartsWith("-")) state = false;

                            if (!state.HasValue)
                            {
                                throw new CommandInvocationException("Unknown setting flag");
                            }

                            var flag = x.Substring(1);
                            switch (flag.ToLowerInvariant())
                            {
                                case "showwikilink":
                                    categoryWatcher.ShowWikiLink = state.Value;
                                    break;
                                case "showshorturl":
                                    categoryWatcher.ShowShortUrl = state.Value;
                                    break;
                                case "showwaittime":
                                    categoryWatcher.ShowWaitTime = state.Value;
                                    break;
                                case "delta":
                                    categoryWatcher.Delta = state.Value;
                                    break;
                                default:
                                    throw new CommandInvocationException("Unknown setting");
                            }
                        });

                this.DatabaseSession.Update(categoryWatcher);

                yield return new CommandResponse { Message = "Configuration for watcher updated." };
            }
            else
            {
                yield return new CommandResponse { Message = "No changes to configuration specified" };
            }
        }

        private IEnumerable<CommandResponse> SetWatcherConfig(string watcher)
        {
            var categoryWatcher = this.GetCategoryWatcher(watcher);

            var textType = this.Arguments.Skip(2).First();

            var textContent = string.Join(" ", this.Arguments.Skip(3));
            int value;

            switch (textType)
            {
                case "singular":
                    categoryWatcher.ItemSingular = textContent;
                    break;
                case "plural":
                    categoryWatcher.ItemPlural = textContent;
                    break;
                case "action":
                    categoryWatcher.ItemAction = textContent;
                    break;
                case "category":
                    categoryWatcher.Category = textContent;
                    break;
                case "priority":
                    if (!int.TryParse(textContent, out value))
                    {
                        throw new CommandInvocationException("set");
                    }
                    categoryWatcher.Priority = value;
                    break;
                case "mintime":
                    if (!int.TryParse(textContent, out value))
                    {
                        throw new CommandInvocationException("set");
                    }
                    categoryWatcher.Priority = value;
                    break;
                default:
                    throw new CommandInvocationException("Unknown text type, expecting singular, plural, or action");
            }

            this.DatabaseSession.Update(categoryWatcher);

            yield return new CommandResponse { Message = "Configuration for watcher updated." };
        }

        private CategoryWatcher GetCategoryWatcher(string watcher)
        {
            var list =
                this.DatabaseSession.CreateCriteria<CategoryWatcher>()
                    .Add(Restrictions.Eq("Channel", this.CommandChannel))
                    .Add(Restrictions.Eq("Keyword", watcher))
                    .List<CategoryWatcher>();

            if (list.Count != 1)
            {
                throw new CommandErrorException("Ambiguous watcher definition");
            }

            var categoryWatcher = list.First();
            return categoryWatcher;
        }
        
        // /// <returns>
        // /// </param>
        // /// The watcher.
        // /// <param name="watcher">
        // /// </summary>
        // /// The status.
        // /// <summary>
        // /// The <see cref="IEnumerable{CommandResponse}"/>.
        // /// </returns>
        // /// <remarks>
        // /// TODO: Rewrite! this isn't clear or concise at all.
        // /// </remarks>
        // private IEnumerable<CommandResponse> Status(string watcher)
        // {
        // string[] messageParams =
        // {
        // watcher, 
        // WatcherController.Instance().IsWatcherInChannel(this.CommandSource, watcher)
        // ? this.CommandServiceHelper.MessageService.RetrieveMessage(
        // Messages.Enabled, 
        // this.CommandSource, 
        // null)
        // : this.CommandServiceHelper.MessageService.RetrieveMessage(
        // Messages.Disabled, 
        // this.CommandSource, 
        // null), 
        // WatcherController.Instance().GetDelay(watcher).ToString(CultureInfo.InvariantCulture)
        // };
        // var message = this.CommandServiceHelper.MessageService.RetrieveMessage(
        // "keywordStatus",
        // this.CommandSource,
        // messageParams);
        // return new CommandResponse { Message = message }.ToEnumerable();
        // }
    }
}