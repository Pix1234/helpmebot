// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HelpMessage.cs" company="Helpmebot Development Team">
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
//   The help message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.CommandUtilities.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.ExtensionMethods;

    /// <summary>
    /// The help message.
    /// </summary>
    public class HelpMessage
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="HelpMessage"/> class. 
        /// </summary>
        /// <param name="commandName">
        /// The command Name.
        /// </param>
        /// <param name="syntax">
        /// The syntax.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public HelpMessage(string commandName, string syntax, string text)
            : this(commandName, syntax.ToEnumerable(), text.ToEnumerable())
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="HelpMessage"/> class.
        /// </summary>
        /// <param name="commandName">
        /// The command name.
        /// </param>
        /// <param name="syntax">
        /// The syntax.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public HelpMessage(string commandName, IEnumerable<string> syntax, string text)
            : this(commandName, syntax, text.ToEnumerable())
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="HelpMessage"/> class.
        /// </summary>
        /// <param name="commandName">
        /// The command name.
        /// </param>
        /// <param name="syntax">
        /// The syntax.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public HelpMessage(string commandName, string syntax, IEnumerable<string> text)
            : this(commandName, syntax.ToEnumerable(), text)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="HelpMessage"/> class.
        /// </summary>
        /// <param name="commandName">
        /// The command name.
        /// </param>
        /// <param name="syntax">
        /// The syntax.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public HelpMessage(string commandName, IEnumerable<string> syntax, IEnumerable<string> text)
        {
            this.CommandName = commandName;
            this.Syntax = syntax;
            this.Text = text;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string CommandName { get; private set; }

        /// <summary>
        /// Gets the syntax.
        /// </summary>
        public IEnumerable<string> Syntax { get; private set; }

        /// <summary>
        /// Gets the text.
        /// </summary>
        public IEnumerable<string> Text { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The to command responses.
        /// </summary>
        /// <param name="commandTrigger">
        /// The command Trigger.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        public IEnumerable<CommandResponse> ToCommandResponses(string commandTrigger)
        {
            var messages = new List<CommandResponse>();

            messages.AddRange(
                this.Syntax.Select(
                    syntax =>
                    new CommandResponse
                        {
                            Message =
                                string.Format("{2}{0} {1}", this.CommandName, syntax, commandTrigger), 
                            Destination = CommandResponseDestination.PrivateMessage
                        }));

            messages.AddRange(
                this.Text.Select(
                    helpText =>
                    new CommandResponse
                        {
                            Message = string.Format("   {0}", helpText), 
                            Destination = CommandResponseDestination.PrivateMessage
                        }));

            return messages;
        }

        #endregion
    }
}