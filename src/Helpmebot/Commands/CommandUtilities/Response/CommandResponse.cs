// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandResponse.cs" company="Helpmebot Development Team">
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
//   The individual response
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.CommandUtilities.Response
{
    using System.Collections.Generic;
    using System.Linq;

    using Helpmebot.ExtensionMethods;

    /// <summary>
    /// The individual response
    /// </summary>
    public class CommandResponse
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the client to client protocol.
        /// </summary>
        public string ClientToClientProtocol { get; set; }

        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        public CommandResponseDestination Destination { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the directed to.
        /// </summary>
        public IEnumerable<string> RedirectionTarget { get; set; }

        /// <summary>
        /// The type of response
        /// </summary>
        public CommandResponseType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ignore redirection.
        /// </summary>
        public bool IgnoreRedirection { get; set; }

        #endregion

        /// <summary>
        /// The compile message.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string CompileMessage()
        {
            string message = this.Message;

            if (this.ClientToClientProtocol != null)
            {
                message = message.SetupForCtcp(this.ClientToClientProtocol);
            }
            else
            {
                if (!this.IgnoreRedirection && this.RedirectionTarget != null && this.RedirectionTarget.Any())
                {
                    message = string.Format("{0}: {1}", this.RedirectionTarget.Implode(", "), message);
                }
            }

            return message;
        }
    }
}