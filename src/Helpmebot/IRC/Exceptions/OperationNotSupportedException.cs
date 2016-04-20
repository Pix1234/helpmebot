// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationNotSupportedException.cs" company="Helpmebot Development Team">
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
//   The operation not supported exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Exceptions
{
    using System;

    /// <summary>
    /// The operation not supported exception.
    /// </summary>
    public class OperationNotSupportedException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ApplicationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error. </param>
        public OperationNotSupportedException(string message)
            : base(message)
        {
        }
    }
}
