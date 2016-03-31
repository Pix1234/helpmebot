// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessLogEntry.cs" company="Helpmebot Development Team">
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
//   The access log entry.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Model
{
    using System;

    using Helpmebot.Persistence;

    /// <summary>
    /// The access log entry.
    /// </summary>
    public class AccessLogEntry : EntityBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        public virtual string Account { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        public virtual string Arguments { get; set; }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        public virtual string Channel { get; set; }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public virtual string Command { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether execution allowed.
        /// </summary>
        public virtual bool ExecutionAllowed { get; set; }

        /// <summary>
        /// Gets or sets the required flag.
        /// </summary>
        public virtual string RequiredFlag { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public virtual DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the user flags.
        /// </summary>
        public virtual string UserFlags { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public virtual string UserIdentifier { get; set; }

        #endregion
    }
}