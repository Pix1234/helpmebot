// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivateMessage.cs" company="Helpmebot Development Team">
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
//   The private message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Messages
{
    using System.Collections.Generic;

    /// <summary>
    /// The private message.
    /// </summary>
    internal class PrivateMessage : Message
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="PrivateMessage"/> class.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public PrivateMessage(string destination, string message)
            : base("PRIVMSG", new List<string> { destination, message })
        {
        }

        #endregion
    }
}