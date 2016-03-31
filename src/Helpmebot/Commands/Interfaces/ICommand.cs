// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommand.cs" company="Helpmebot Development Team">
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
//   The Command interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;

    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    using NHibernate;

    /// <summary>
    /// The Command interface.
    /// </summary>
    public interface ICommand
    {
        #region Public Properties

        /// <summary>
        /// Gets the arguments to the command.
        /// </summary>
        IEnumerable<string> Arguments { get; }

        /// <summary>
        /// Gets the source (where the command was triggered).
        /// </summary>
        string CommandSource { get; }

        /// <summary>
        /// Gets the flag required to execute.
        /// </summary>
        string Flag { get; }

        /// <summary>
        /// Gets or sets the original arguments.
        /// </summary>
        IEnumerable<string> OriginalArguments { get; set; }

        /// <summary>
        /// Gets or sets the redirection target.
        /// </summary>
        IEnumerable<string> RedirectionTarget { get; set; }

        /// <summary>
        /// Gets the user who triggered the command.
        /// </summary>
        IUser User { get; }

        /// <summary>
        /// Gets or sets the command message.
        /// </summary>
        CommandMessage CommandMessage { get; set; }

        /// <summary>
        /// Gets the database session.
        /// </summary>
        ISession DatabaseSession { get; }

        /// <summary>
        /// Gets the command channel.
        /// </summary>
        Channel CommandChannel { get; }

        /// <summary>
        /// The commandCompletedSemaphore.
        /// </summary>
        Semaphore CommandCompletedSemaphore { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns true if the command can be executed.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool CanExecute();

        /// <summary>
        /// The help message.
        /// </summary>
        /// <param name="helpKey">
        /// The help Key.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        IEnumerable<CommandResponse> HelpMessage(string helpKey = null);

        /// <summary>
        /// The run.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        IEnumerable<CommandResponse> Run();

        #endregion
    }
}