// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessCommand.cs" company="Helpmebot Development Team">
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
//   The access command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.AccessControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.IRC.Model;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Repositories;

    using NHibernate;

    /// <summary>
    /// The access command.
    /// </summary>
    [CommandFlag(Model.Flag.Access)]
    [CommandInvocation("access")]
    public class AccessCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="AccessCommand"/> class.
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
        public AccessCommand(
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
            if (this.Arguments.Count() < 3)
            {
                throw new ArgumentCountException(3, this.Arguments.Count());
            }

            var mode = this.Arguments.ElementAt(0).ToLowerInvariant();
            var flagGroup = this.Arguments.ElementAt(1).ToLowerInvariant();
            var mask = this.Arguments.ElementAt(2).ToLowerInvariant();

            if (mode == "add")
            {
                return this.AddGroup(flagGroup, mask);
            }

            if (mode == "del" || mode == "delete")
            {
                return this.DeleteGroup(flagGroup, mask);
            }

            throw new CommandInvocationException();
        }

        /// <summary>
        /// The help.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IDictionary<string, HelpMessage> Help()
        {
            var addSyntax = new[] { "add <GroupName> <HostMask>", "add <GroupName> $a:<AccountName>", "add <GroupName> <Channel>" };
            var deleteSyntax = new[] { "delete <GroupName> <HostMask>", "delete <GroupName> $a:<AccountName>", "delete <GroupName> <Channel>" };
            return new Dictionary<string, HelpMessage>
                       {
                           {
                               "add", 
                               new HelpMessage(
                               this.CommandName, 
                               addSyntax, 
                               "Creates or edits a flag group.")
                           }, 
                           {
                               "delete", 
                               new HelpMessage(
                               this.CommandName, 
                               deleteSyntax, 
                               "Deletes an existing flag group.")
                           }
                       };
        }

        /// <summary>
        /// The add group.
        /// </summary>
        /// <param name="flagGroup">
        /// The flag group.
        /// </param>
        /// <param name="mask">
        /// The mask.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        private IEnumerable<CommandResponse> AddGroup(string flagGroup, string mask)
        {
            var group = this.DatabaseSession.QueryOver<FlagGroup>().Where(x => x.Name == flagGroup).SingleOrDefault();
            if (group == null)
            {
                throw new CommandErrorException("Unknown flag group");
            }

            if (mask.StartsWith("#"))
            {
                var flagGroupChannel = new FlagGroupChannel();

                var channel = this.CommandServiceHelper.ChannelRepository.GetByName(mask, this.DatabaseSession);
                if (channel == null)
                {
                    this.Logger.InfoFormat("Cannot find channel {0} in configuration, creating (default disabled)", mask);
                    channel = new Channel { Name = mask, Enabled = false };
                    this.DatabaseSession.Save(channel);
                }

                flagGroupChannel.Channel = channel;
                flagGroupChannel.FlagGroup = group;

                this.DatabaseSession.Save(flagGroupChannel);
                this.CommandServiceHelper.UserFlagService.InvalidateCache();

                yield return new CommandResponse { Message = string.Format("Added to channel {0}", flagGroupChannel) };
            }
            else
            {
                var flagGroupUser = new FlagGroupUser();
                IrcUser user;

                try
                {
                    user = IrcUser.Parse(mask);
                }
                catch (Exception e)
                {
                    this.Logger.InfoFormat("Encountered problem parsing mask {0}", mask);
                    throw new CommandErrorException("Could not parse the specified mask.", e);
                }

                flagGroupUser.Account = user.Account ?? "*";
                flagGroupUser.Nickname = user.Nickname ?? "*";
                flagGroupUser.Username = user.Username ?? "*";
                flagGroupUser.Hostname = user.Hostname ?? "*";

                flagGroupUser.FlagGroup = group;

                this.DatabaseSession.Save(flagGroupUser);

                this.CommandServiceHelper.UserFlagService.InvalidateCache();

                yield return new CommandResponse { Message = string.Format("Added to user mask {0}", flagGroupUser) };
            }
        }

        /// <summary>
        /// The delete group.
        /// </summary>
        /// <param name="flagGroup">
        /// The flag group.
        /// </param>
        /// <param name="mask">
        /// The mask.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        private IEnumerable<CommandResponse> DeleteGroup(string flagGroup, string mask)
        {
            var group = this.DatabaseSession.QueryOver<FlagGroup>().Where(x => x.Name == flagGroup).SingleOrDefault();
            if (group == null)
            {
                throw new CommandErrorException("Unknown flag group");
            }

            if (mask.StartsWith("#"))
            {
                var channel = this.CommandServiceHelper.ChannelRepository.GetByName(mask, this.DatabaseSession);
                if (channel == null)
                {
                    this.Logger.InfoFormat("Cannot find channel {0} in configuration", mask);
                    throw new CommandErrorException("Could not find the specified channel in the configuration.");
                }

                /* ReSharper disable PossibleUnintendedReferenceComparison */
                var flagGroupChannel =
                    this.DatabaseSession.QueryOver<FlagGroupChannel>()
                        .Where(x => x.Channel == channel && x.FlagGroup == group).SingleOrDefault();
                /* ReSharper restore PossibleUnintendedReferenceComparison */

                if (flagGroupChannel.IsProtected)
                {
                    throw new CommandErrorException("Cannot delete protected access entry.");
                }

                this.DatabaseSession.Delete(flagGroupChannel);
                this.CommandServiceHelper.UserFlagService.InvalidateCache();

                yield return new CommandResponse { Message = string.Format("Deleted from channel {0}", flagGroupChannel) };
            }
            else
            {

                IrcUser user;

                try
                {
                    user = IrcUser.Parse(mask);
                }
                catch (Exception e)
                {
                    this.Logger.InfoFormat("Encountered problem parsing mask {0}", mask);
                    throw new CommandErrorException("Could not parse the specified mask.", e);
                }

                var flagGroupUser =
                    this.DatabaseSession.QueryOver<FlagGroupUser>()
                        .Where(
                            x =>
                            x.Account == (user.Account ?? "*") && x.Nickname == (user.Nickname ?? "*")
                            && x.Username == (user.Username ?? "*") && x.Hostname == (user.Hostname ?? "*")
                            // ReSharper disable once PossibleUnintendedReferenceComparison
                            && x.FlagGroup == group).SingleOrDefault();

                if (flagGroupUser.Protected)
                {
                    throw new CommandErrorException("Cannot delete protected access entry.");
                }

                this.DatabaseSession.Delete(flagGroupUser);

                this.CommandServiceHelper.UserFlagService.InvalidateCache();

                yield return
                    new CommandResponse { Message = string.Format("Deleted from user mask {0}", flagGroupUser) };
            }
        }

        #endregion
    }
}