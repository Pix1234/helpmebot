// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWikiLinkService.cs" company="Helpmebot Development Team">
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
//   The WikiLinkService interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Helpmebot.IRC.Events;

    /// <summary>
    /// The WikiLinkService interface.
    /// </summary>
    public interface IWikiLinkService
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get last for channel.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        IEnumerable<string> GetLastForChannel(string channel);

        /// <summary>
        /// The get link.
        /// </summary>
        /// <param name="wikilink">
        /// The wikilink.
        /// </param>
        /// <returns>
        /// The <see cref="Uri"/>.
        /// </returns>
        Uri GetLink(string wikilink);

        /// <summary>
        /// Parses text and returns the page titles in any found wikilinks.
        /// </summary>
        /// <param name="input">
        /// The input text
        /// </param>
        /// <returns>
        /// A collection of page titles
        /// </returns>
        IEnumerable<string> ParseForLinks(string input);

        /// <summary>
        /// The parse incoming message.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        void ParseIncomingMessage(object sender, MessageReceivedEventArgs e);

        #endregion
    }
}