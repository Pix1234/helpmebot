// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentCountException.cs" company="Helpmebot Development Team">
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
//   The argument count exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Exceptions
{
    using System;

    /// <summary>
    /// The argument count exception.
    /// </summary>
    [Serializable]
    internal class ArgumentCountException : CommandExecutionException
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="ArgumentCountException"/> class.
        /// </summary>
        /// <param name="expectedCount">
        /// The expected count.
        /// </param>
        /// <param name="actualCount">
        /// The actual count.
        /// </param>
        /// <remarks>
        /// You probably want the other constructor
        /// </remarks>
        public ArgumentCountException(int expectedCount, int actualCount)
            : this(expectedCount, actualCount, null)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ArgumentCountException"/> class.
        /// </summary>
        /// <param name="expectedCount">
        /// The expected count.
        /// </param>
        /// <param name="actualCount">
        /// The actual count.
        /// </param>
        /// <param name="helpKey">
        /// The help Key.
        /// </param>
        public ArgumentCountException(int expectedCount, int actualCount, string helpKey)
            : base(
                string.Format("Insufficient arguments to command. Expected {0}, got {1}.", expectedCount, actualCount))
        {
            this.HelpKey = helpKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the help key.
        /// </summary>
        public string HelpKey { get; private set; }

        #endregion
    }
}