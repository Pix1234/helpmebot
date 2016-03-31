// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommandHandler.cs" company="Helpmebot Development Team">
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
//   The i command handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands.Interfaces
{
    using Helpmebot.IRC.Events;

    /// <summary>
    /// The i command handler.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Called on new messages received by the IRC client
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        void OnMessageReceived(object sender, MessageReceivedEventArgs e);
    }
}