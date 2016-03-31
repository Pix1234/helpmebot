// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HelpCommand.cs" company="Helpmebot Development Team">
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
//   The help command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands
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
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    /// <summary>
    /// The help command.
    /// </summary>
    [CommandFlag(Model.Flag.Standard)]
    [CommandInvocation("help")]
    public class HelpCommand : CommandBase
    {
        #region Fields

        /// <summary>
        /// The command parser.
        /// </summary>
        private readonly ICommandParser commandParser;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="HelpCommand"/> class.
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
        /// <param name="commandParser">
        /// The command Parser.
        /// </param>
        public HelpCommand(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            ILogger logger, 
            ISession databaseSession, 
            ICommandServiceHelper commandServiceHelper, 
            ICommandParser commandParser)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
            this.commandParser = commandParser;
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
            if (!this.Arguments.Any())
            {
                throw new CommandInvocationException();
            }

            string commandName = this.Arguments.ElementAt(0);
            string key = this.Arguments.Count() > 1 ? this.Arguments.ElementAt(1) : null;

            var command = this.commandParser.GetCommand(
                new CommandMessage { CommandName = commandName }, 
                this.User, 
                this.CommandSource,
                this.CommandServiceHelper.Client);

            if (command == null)
            {
                return
                    new CommandResponse
                        {
                            Message = "The specified command could not be found.", 
                            Destination = CommandResponseDestination.PrivateMessage
                        }.ToEnumerable();
            }

            var helpResponses = command.HelpMessage(key).ToList();

            return helpResponses;
        }

        /// <summary>
        /// The help.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IDictionary<string, HelpMessage> Help()
        {
            return new Dictionary<string, HelpMessage>
                       {
                           {
                               string.Empty, 
                               new HelpMessage(
                               this.CommandName, 
                               "<Command>", 
                               "Returns all available help for the specified command.")
                           }, 
                           {
                               "command", 
                               new HelpMessage(
                               this.CommandName, 
                               "<Command> <SubCommand>", 
                               "Returns the help for the specified subcommand.")
                           }
                       };
        }

        #endregion
    }
}