// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICategoryWatcherBackgroundService.cs" company="Helpmebot Development Team">
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
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Background.Interfaces
{
    using System.Collections.Generic;

    using Castle.Core;

    using Helpmebot.Model;

    /// <summary>
    /// The CategoryWatcherBackgroundService interface.
    /// </summary>
    public interface ICategoryWatcherBackgroundService : IStartable
    {
        /// <summary>
        /// The disable watcher.
        /// </summary>
        /// <param name="watcher">
        /// The watcher.
        /// </param>
        void DisableWatcher(CategoryWatcher watcher);

        /// <summary>
        /// The enable watcher.
        /// </summary>
        /// <param name="watcher">
        /// The watcher.
        /// </param>
        void EnableWatcher(CategoryWatcher watcher);

        /// <summary>
        /// Triggers an update for a category watcher in a specific channel
        /// </summary>
        /// <param name="keyword">The keyword identifying the category watcher</param>
        /// <param name="channel">The channel identifying the category watcher</param>
        void TriggerCategoryWatcherUpdate(string keyword, Channel channel);

        IDictionary<string, CategoryWatcher> GetWatchersForChannel(Channel channel);
    }
}