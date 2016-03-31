// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelRepository.cs" company="Helpmebot Development Team">
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
//   The channel repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories
{
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using NHibernate;

    /// <summary>
    /// The channel repository.
    /// </summary>
    public class ChannelRepository : IChannelRepository
    {
        /// <summary>
        /// The get by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <returns>
        /// The <see cref="Channel"/>.
        /// </returns>
        public Channel GetByName(string name, ISession session)
        {
            return session.QueryOver<Channel>().Where(x => x.Name == name).SingleOrDefault();
        }
    }
}