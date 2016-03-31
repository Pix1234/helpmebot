// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserFlagService.cs" company="Helpmebot Development Team">
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
//   Defines the IUserFlagService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services.Interfaces
{
    using System.Collections.Generic;

    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    using NHibernate;

    /// <summary>
    /// The UserFlagService interface.
    /// </summary>
    public interface IUserFlagService
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the database session.
        /// </summary>
        ISession DatabaseSession { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get flags for user.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        IEnumerable<string> GetFlagsForUser(IUser user);

        #endregion

        /// <summary>
        /// The invalidate cache.
        /// </summary>
        void InvalidateCache();

        /// <summary>
        /// The get flags for channel.
        /// </summary>
        /// <param name="channel">
        /// The channel name.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        IEnumerable<string> GetFlagsForChannel(Channel channel);
    }
}