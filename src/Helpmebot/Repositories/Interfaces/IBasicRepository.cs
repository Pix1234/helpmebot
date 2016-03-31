// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBasicRepository.cs" company="Helpmebot Development Team">
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
//   The BasicRepository interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories.Interfaces
{
    using Helpmebot.Persistence;

    /// <summary>
    /// The BasicRepository interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface IBasicRepository<T>
        where T : GuidEntityBase
    {
    }
}