// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandParser.cs" company="Helpmebot Development Team">
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
//   The command parser.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.TypedFactories;

    using NHibernate;

    /// <summary>
    /// The command parser.
    /// </summary>
    public class CommandParser : ICommandParser
    {
        #region Fields

        /// <summary>
        /// The command factory.
        /// </summary>
        private readonly ICommandTypedFactory commandFactory;

        /// <summary>
        /// The command trigger.
        /// </summary>
        private readonly string commandTrigger;

        /// <summary>
        /// The commands.
        /// </summary>
        private readonly Dictionary<string, Dictionary<CommandRegistration, Type>> commands;

        private readonly Dictionary<string, Dictionary<CommandAliasRegistration, string>> aliases;

        /// <summary>
        /// The keyword service.
        /// </summary>
        private readonly IKeywordService keywordService;

        /// <summary>
        /// The keyword factory.
        /// </summary>
        private readonly IKeywordCommandFactory keywordFactory;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandParser"/> class.
        /// </summary>
        /// <param name="commandTrigger">
        /// The command trigger.
        /// </param>
        /// <param name="commandFactory">
        /// The command Factory.
        /// </param>
        /// <param name="keywordService">
        /// The keyword Service.
        /// </param>
        /// <param name="keywordFactory">
        /// The keyword Factory.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="databaseSession">
        /// Database session for loading the aliases only
        /// </param>
        public CommandParser(
            string commandTrigger,
            ICommandTypedFactory commandFactory,
            IKeywordService keywordService,
            IKeywordCommandFactory keywordFactory,
            ILogger logger,
            ISession databaseSession)
        {
            this.commandTrigger = commandTrigger;
            this.commandFactory = commandFactory;
            this.keywordService = keywordService;
            this.keywordFactory = keywordFactory;
            this.logger = logger;
            var types = Assembly.GetExecutingAssembly().GetTypes();

            this.commands = new Dictionary<string, Dictionary<CommandRegistration, Type>>();
            foreach (var type in types)
            {
                if (!type.IsSubclassOf(typeof(CommandBase)))
                {
                    // Not a new command class;
                    continue;
                }

                var customAttributes = type.GetCustomAttributes(typeof(CommandInvocationAttribute), false);
                if (customAttributes.Length > 0)
                {
                    foreach (var attribute in customAttributes)
                    {
                        var commandName = ((CommandInvocationAttribute)attribute).CommandName;

                        if (commandName != string.Empty)
                        {
                            this.RegisterCommand(commandName, type);
                        }
                    }
                }
            }

            var commandAliases = databaseSession.CreateCriteria<CommandAlias>().List<CommandAlias>();
            this.aliases = new Dictionary<string, Dictionary<CommandAliasRegistration, string>>();
            foreach (var alias in commandAliases)
            {
                this.RegisterCommandAlias(alias.Invocation, alias.Target, alias.Channel.Name);
            }

            this.logger.InfoFormat("Initialised Command Parser with {0} commands and {1} aliases.", this.commands.Count, this.aliases.Count);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get command.
        /// </summary>
        /// <param name="commandMessage">
        /// The command Message.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="ICommand"/>.
        /// </returns>
        public ICommand GetCommand(CommandMessage commandMessage, IUser user, string destination, IIrcClient client)
        {
            if (commandMessage == null || commandMessage.CommandName == null)
            {
                this.logger.Debug("Returning early from GetCommand - null message!");
                return null;
            }

            IEnumerable<string> originalArguments = new List<string>();

            if (commandMessage.ArgumentList != null)
            {
                originalArguments =
                    commandMessage.ArgumentList.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            var redirectionResult = this.ParseRedirection(originalArguments);
            IEnumerable<string> arguments = redirectionResult.Arguments.ToList();

            var commandName = commandMessage.CommandName.ToLower(CultureInfo.InvariantCulture);
            var commandType = this.GetRegisteredCommand(commandName, destination);

            if (commandType != null)
            {   
                this.logger.InfoFormat("Creating command object of type {0}", commandType);

                try
                {
                    var command = this.commandFactory.CreateType(commandType, destination, user, arguments);

                    command.RedirectionTarget = redirectionResult.Target;
                    command.OriginalArguments = originalArguments;
                    command.CommandMessage = commandMessage;

                    return command;
                }
                catch (TargetInvocationException e)
                {
                    this.logger.Error("Unable to create instance of command.", e.InnerException);
                    client.SendMessage("##helpmebot", e.InnerException.Message.Replace("\r\n", " "));
                }
            }

            var keyword = this.keywordService.Get(commandMessage.CommandName);
            if (keyword != null)
            {
                this.logger.InfoFormat("Creating keyword command object for {0}", commandMessage.CommandName);

                ICommand command = this.keywordFactory.CreateKeyword(
                    destination,
                    user,
                    arguments,
                    client,
                    keyword);

                command.RedirectionTarget = redirectionResult.Target;
                command.OriginalArguments = originalArguments;
                command.CommandMessage = commandMessage;

                return command;
            }

            return null;
        }

        private Type GetRegisteredCommand(string commandName, string destination)
        {
            string realCommandName = this.DereferenceAlias(commandName, destination);

            Dictionary<CommandRegistration, Type> commandRegistrationSet;
            if (!this.commands.TryGetValue(realCommandName, out commandRegistrationSet))
            {
                // command doesn't exist anywhere
                return null;
            }

            var channelRegistration = commandRegistrationSet.Keys.FirstOrDefault(x => x.Channel == destination);

            if (channelRegistration != null)
            {
                // This command is defined locally in this channel
                return commandRegistrationSet[channelRegistration];
            }

            var globalRegistration = commandRegistrationSet.Keys.FirstOrDefault(x => x.Channel == null);

            if (globalRegistration != null)
            {
                // This command is not defined locally, but is defined globally
                return commandRegistrationSet[globalRegistration];
            }

            // This command has a registration entry, but isn't defined in this channel or globally.
            return null;
        }

        private string DereferenceAlias(string commandName, string destination)
        {
            Dictionary<CommandAliasRegistration, string> commandRegistrationSet;
            if (!this.aliases.TryGetValue(commandName, out commandRegistrationSet))
            {
                // alias doesn't exist anywhere, return original command name
                return commandName;
            }

            var channelRegistration = commandRegistrationSet.Keys.FirstOrDefault(x => x.Channel == destination);

            if (channelRegistration != null)
            {
                // This alias is defined locally in this channel
                return commandRegistrationSet[channelRegistration];
            }

            var globalRegistration = commandRegistrationSet.Keys.FirstOrDefault(x => x.Channel == null);

            if (globalRegistration != null)
            {
                // This alias is not defined locally, but is defined globally
                return commandRegistrationSet[globalRegistration];
            }

            // This command has a registration entry, but isn't defined in this channel or globally.
            return commandName;
        }

        /// <summary>
        /// The parse redirection.
        /// </summary>
        /// <param name="inputArguments">
        /// The input arguments.
        /// </param>
        /// <returns>
        /// The <see cref="RedirectionResult"/>.
        /// </returns>
        public RedirectionResult ParseRedirection(IEnumerable<string> inputArguments)
        {
            var targetList = new List<string>();
            var parsedArguments = new List<string>();

            bool redirecting = false;

            foreach (var argument in inputArguments)
            {
                if (redirecting)
                {
                    redirecting = false;
                    targetList.Add(argument);
                    continue;
                }

                if (argument == ">")
                {
                    redirecting = true;
                    continue;
                }

                if (argument.StartsWith(">"))
                {
                    targetList.Add(argument.Substring(1));
                    continue;
                }

                parsedArguments.Add(argument);
            }

            // last word on line was >
            if (redirecting)
            {
                parsedArguments.Add(">");
            }

            return new RedirectionResult(parsedArguments, targetList);
        }

        /// <summary>
        /// The parse command message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <returns>
        /// The <see cref="CommandMessage"/>.
        /// </returns>
        public CommandMessage ParseCommandMessage(string message, string nickname)
        {
            var validCommand =
                new Regex(
                    @"^(?:" + this.commandTrigger + @"(?:(?<botname>(?:" + nickname + @")|(?:"
                    + nickname.ToLower() + @")) )?(?<cmd>[" + "0-9a-z-_" + "]+)|(?<botname>(?:" + nickname
                    + @")|(?:" + nickname.ToLower() + @"))[ ,>:](?: )?(?<cmd>[" + "0-9a-z-_"
                    + "]+))(?: )?(?<args>.*?)(?:\r)?$");

            Match m = validCommand.Match(message);

            if (m.Length > 0)
            {
                var commandMessage = new CommandMessage();

                if (m.Groups["botname"].Length > 0)
                {
                    commandMessage.OverrideSilence = true;
                }

                if (m.Groups["cmd"].Length > 0)
                {
                    commandMessage.CommandName = m.Groups["cmd"].Value.Trim();
                }
                else
                {
                    return null;
                }

                if (m.Groups["args"].Length > 0)
                {
                    commandMessage.ArgumentList = m.Groups["args"].Length > 0
                                                      ? m.Groups["args"].Value.Trim()
                                                      : string.Empty;
                }
                else
                {
                    commandMessage.ArgumentList = string.Empty;
                }

                return commandMessage;
            }

            return null;
        }

        /// <summary>
        /// The release.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        public void Release(ICommand command)
        {
            this.commandFactory.Release(command);
        }

        #endregion

        #region Command Registration

        /// <summary>
        /// The register command.
        /// </summary>
        /// <param name="commandName">
        /// The keyword.
        /// </param>
        /// <param name="implementation">
        /// The implementation.
        /// </param>
        public void RegisterCommand(string commandName, Type implementation)
        {
            this.RegisterCommand(commandName, implementation, null);
        }

        /// <summary>
        /// The register command.
        /// </summary>
        /// <param name="commandName">
        /// The keyword.
        /// </param>
        /// <param name="implementation">
        /// The implementation.
        /// </param>
        /// <param name="channel">
        /// The channel to limit this registration to
        /// </param>
        public void RegisterCommand(string commandName, Type implementation, string channel)
        {
            if (!this.commands.ContainsKey(commandName))
            {
                this.commands.Add(commandName, new Dictionary<CommandRegistration, Type>());
            }

            this.commands[commandName].Add(new CommandRegistration(channel, implementation), implementation);
        }
        
        public void RegisterCommandAlias(string aliasName, string targetName, string channel)
        {
            if (this.GetRegisteredCommand(targetName, channel) == null)
            {
                throw new Exception("Cannot register alias for unknown command");
            }

            if (!this.aliases.ContainsKey(aliasName))
            {
                this.aliases.Add(aliasName, new Dictionary<CommandAliasRegistration, string>());
            }

            this.aliases[aliasName].Add(new CommandAliasRegistration(channel, targetName), targetName);
        }

        public void UnregisterCommandAlias(string aliasName, string channel)
        {
            if (!this.aliases.ContainsKey(aliasName))
            {
                throw new Exception("Cannot unregister alias - alias is not registered");
            }

            var commandAliasRegistration = this.aliases[aliasName].Keys.FirstOrDefault(x => x.Channel == channel);

            if (commandAliasRegistration == null)
            {
                throw new Exception("Cannot unregister alias - alias is not registered");
            }

            this.aliases[aliasName].Remove(commandAliasRegistration);
        }


        #endregion
    }
}