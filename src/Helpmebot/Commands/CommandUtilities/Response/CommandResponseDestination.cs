// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandResponseDestination.cs" company="Helpmebot Development Team">
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
//   Command response destinations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands.CommandUtilities.Response
{
    /// <summary>
    ///     Command response destinations
    /// </summary>
    public enum CommandResponseDestination
    {
        /// <summary>
        ///     Back to the channel from whence it came
        /// </summary>
        Default, 

        /// <summary>
        ///     To the debugging channel
        /// </summary>
        ChannelDebug, 

        /// <summary>
        ///     Back to the user in a private message
        /// </summary>
        PrivateMessage
    }
}