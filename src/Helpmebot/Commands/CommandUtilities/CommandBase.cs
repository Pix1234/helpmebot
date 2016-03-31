// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandBase.cs" company="Helpmebot Development Team">
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
//   Defines the CommandBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.CommandUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading;

    using Castle.Core.Internal;
    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    using NHibernate;

    /// <summary>
    /// The command base.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        #region Fields

        /// <summary>
        /// The command service helper.
        /// </summary>
        private readonly ICommandServiceHelper commandServiceHelper;

        /// <summary>
        /// The configuration helper.
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;
        
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandBase"/> class.
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
        protected CommandBase(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            ILogger logger, 
            ISession databaseSession, 
            ICommandServiceHelper commandServiceHelper)
        {
            this.commandServiceHelper = commandServiceHelper;
            this.configurationHelper = commandServiceHelper.ConfigurationHelper;
            this.DatabaseSession = databaseSession;
            this.CommandCompletedSemaphore = new Semaphore(1, 1);
            this.Logger = logger;
            this.CommandSource = commandSource;
            this.User = user;
            this.Arguments = arguments;
            
            this.CommandChannel = this.CommandServiceHelper.ChannelRepository.GetByName(commandSource, databaseSession);
        }

        #endregion

        #region Public Properties

        public bool Executed { get; private set; }

        /// <summary>
        /// Gets the command Completed Semaphore.
        /// <para>
        /// This semaphore can be set with <code>CommandCompletedSemaphore.WaitOne()</code> to suspend destruction of the command for a maximum of thirty seconds.
        /// </para>
        /// </summary>
        public Semaphore CommandCompletedSemaphore { get; private set; }

        /// <summary>
        /// Gets the arguments to the command.
        /// </summary>
        public IEnumerable<string> Arguments { get; private set; }

        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string CommandName
        {
            get
            {
                var customAttributes = this.GetType().GetCustomAttributes(typeof(CommandInvocationAttribute), false);
                if (customAttributes.Length > 0)
                {
                    return ((CommandInvocationAttribute)customAttributes.First()).CommandName;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the command service helper.
        /// </summary>
        public ICommandServiceHelper CommandServiceHelper
        {
            get
            {
                return this.commandServiceHelper;
            }
        }

        /// <summary>
        /// Gets the source (where the command was triggered).
        /// </summary>
        public string CommandSource { get; private set; }

        /// <summary>
        /// Gets the command channel.
        /// </summary>
        public Channel CommandChannel { get; private set; }

        /// <summary>
        /// Gets the database session.
        /// </summary>
        public ISession DatabaseSession { get; private set; }

        /// <summary>
        /// Gets the flag required to execute.
        /// </summary>
        public string Flag
        {
            get
            {
                return this.GetType().GetAttribute<CommandFlagAttribute>().Flag;
            }
        }

        /// <summary>
        /// Gets or sets the original arguments.
        /// </summary>
        public IEnumerable<string> OriginalArguments { get; set; }

        /// <summary>
        /// Gets or sets the redirection target.
        /// </summary>
        public IEnumerable<string> RedirectionTarget { get; set; }

        /// <summary>
        /// Gets the user who triggered the command.
        /// </summary>
        public IUser User { get; private set; }

        /// <summary>
        /// Gets or sets the command message.
        /// </summary>
        public CommandMessage CommandMessage { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns true if the command can be executed.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public virtual bool CanExecute()
        {
            var flagsForUser = this.CommandServiceHelper.UserFlagService.GetFlagsForUser(this.User);
            var flagsForChannel = this.CommandServiceHelper.UserFlagService.GetFlagsForChannel(this.CommandChannel);

            return flagsForUser.Contains(this.Flag) || flagsForChannel.Contains(this.Flag);
        }

        /// <summary>
        /// The help message.
        /// </summary>
        /// <param name="helpKey">
        /// The help Key.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        public IEnumerable<CommandResponse> HelpMessage(string helpKey = null)
        {
            var helpMessages = this.Help();

            var commandTrigger = this.configurationHelper.CoreConfiguration.CommandTrigger;

            if (helpMessages == null)
            {
                return new List<CommandResponse>();
            }

            if (helpKey != null && helpMessages.ContainsKey(helpKey))
            {
                return helpMessages[helpKey].ToCommandResponses(commandTrigger);
            }

            var help = new List<CommandResponse>();
            foreach (var helpMessage in helpMessages)
            {
                help.AddRange(helpMessage.Value.ToCommandResponses(commandTrigger));
            }

            return help;
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        public IEnumerable<CommandResponse> Run()
        {
            if (this.Executed)
            {
                throw new Exception("Already executed instance of command");
            }

            try
            {
                // TODO: check what should be in a transaction, and what should not - autonomous transactions?
                this.DatabaseSession.FlushMode = FlushMode.Commit;
                ITransaction transaction = this.DatabaseSession.BeginTransaction(IsolationLevel.ReadCommitted);

                if (this.CanExecute())
                {
                    this.CommandServiceHelper.AccessLogService.Success(
                        this.User,
                        this.GetType(),
                        this.Arguments,
                        this.CommandSource);

                    try
                    {
                        var commandResponses = this.Execute() ?? new List<CommandResponse>();
                        var completedResponses = this.OnCompleted() ?? new List<CommandResponse>();

                        // Resolve the list into a concrete list before committing the transaction.
                        var responses = commandResponses.Concat(completedResponses).ToList();

                        if (transaction.IsActive)
                        {
                            transaction.Commit();
                        }

                        return responses;
                    }
                    catch (CommandInvocationException e)
                    {
                        this.Logger.Info("Command encountered an issue from invocation.");

                        if (transaction.IsActive)
                        {
                            transaction.Rollback();
                        }

                        return this.HelpMessage(e.HelpKey);
                    }
                    catch (ArgumentCountException e)
                    {
                        this.Logger.Info("Command executed with missing arguments.");

                        var responses = new List<CommandResponse>
                                            {
                                                new CommandResponse
                                                    {
                                                        Destination =
                                                            CommandResponseDestination
                                                            .Default,
                                                        Message = e.Message
                                                    }
                                            };

                        responses.AddRange(this.HelpMessage(e.HelpKey));

                        if (transaction.IsActive)
                        {
                            transaction.Rollback();
                        }

                        return responses;
                    }
                    catch (CommandExecutionException e)
                    {
                        this.Logger.Warn("Command encountered an issue during execution.", e);

                        if (transaction.IsActive)
                        {
                            transaction.Rollback();
                        }

                        return new List<CommandResponse>
                                   {
                                       new CommandResponse
                                           {
                                               Destination =
                                                   CommandResponseDestination
                                                   .Default,
                                               Message = e.Message
                                           }
                                   };
                    }
                    catch (ADOException e)
                    {
                        this.Logger.Error("Command encountered an issue during execution.", e);

                        if (transaction.IsActive)
                        {
                            transaction.Rollback();
                        }

                        return new List<CommandResponse>
                                   {
                                       new CommandResponse
                                           {
                                               Destination =
                                                   CommandResponseDestination
                                                   .Default,
                                               Message = e.Message
                                           }
                                   };
                    }
                    catch (Exception e)
                    {
                        this.Logger.Error("Command encountered an issue during execution.", e);

                        if (transaction.IsActive)
                        {
                            transaction.Rollback();
                        }

                        return new List<CommandResponse>
                                   {
                                       new CommandResponse
                                           {
                                               Destination =
                                                   CommandResponseDestination
                                                   .Default,
                                               Message = e.Message
                                           }
                                   };
                    }
                }

                this.CommandServiceHelper.AccessLogService.Failure(
                    this.User,
                    this.GetType(),
                    this.Arguments,
                    this.CommandSource);

                this.Logger.InfoFormat("Access denied for user {0}", this.User);

                IEnumerable<CommandResponse> accessDeniedResponses = this.OnAccessDenied()
                                                                     ?? new List<CommandResponse>();

                if (transaction.IsActive)
                {
                    transaction.Commit();
                }

                return accessDeniedResponses;
            }
            finally
            {
                this.Executed = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected abstract IEnumerable<CommandResponse> Execute();

        /// <summary>
        /// The help.
        /// </summary>
        /// <returns>
        /// The <see cref="IDictionary{String, HelpMessage}"/>.
        /// </returns>
        protected virtual IDictionary<string, HelpMessage> Help()
        {
            return new Dictionary<string, HelpMessage>
                       {
                           {
                               string.Empty, 
                               new HelpMessage(
                               this.CommandName, 
                               string.Empty, 
                               "No help is available for this command.")
                           }
                       };
        }

        /// <summary>
        /// The on access denied.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected virtual IEnumerable<CommandResponse> OnAccessDenied()
        {
            var response = new CommandResponse
                               {
                                   Destination = CommandResponseDestination.PrivateMessage, 
                                   Message =
                                       this.CommandServiceHelper.MessageService.RetrieveMessage(
                                           Messages.OnAccessDenied, 
                                           this.CommandSource, 
                                           null)
                               };

            return response.ToEnumerable();
        }

        /// <summary>
        /// The on completed.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected virtual IEnumerable<CommandResponse> OnCompleted()
        {
            return null;
        }

        #endregion
    }
}