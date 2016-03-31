// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandExecutionException.cs" company="Helpmebot Development Team">
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
//   The command execution exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Exceptions
{
    using System;

    /// <summary>
    /// The command execution exception.
    /// </summary>
    [Serializable]
    internal abstract class CommandExecutionException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandExecutionException"/> class. 
        /// </summary>
        /// <param name="message">
        /// The message that describes the error. 
        /// </param>
        protected CommandExecutionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandExecutionException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        protected CommandExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandExecutionException"/> class. 
        /// </summary>
        protected CommandExecutionException()
        {
        }

        #endregion
    }
}