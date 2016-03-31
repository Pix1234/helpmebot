// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IKeywordCommandFactory.cs" company="Helpmebot Development Team">
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
//   The KeywordCommandFactory interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.TypedFactories
{
    using System.Collections.Generic;

    using Helpmebot.Commands;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The KeywordCommandFactory interface.
    /// </summary>
    public interface IKeywordCommandFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="commandSource">
        /// The command source.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <returns>
        /// The <see cref="KeywordRetrieveCommand"/>.
        /// </returns>
        KeywordRetrieveCommand CreateKeyword(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            IIrcClient client, 
            Keyword keyword);

        /// <summary>
        /// The release.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        void Release(KeywordRetrieveCommand command);

        #endregion
    }
}