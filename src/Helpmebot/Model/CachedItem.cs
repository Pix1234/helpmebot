// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachedItem.cs" company="Helpmebot Development Team">
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
//   The cached item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Model
{
    using System;

    /// <summary>
    /// The cached item.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the cached item
    /// </typeparam>
    public class CachedItem<T>
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CachedItem{T}"/> class.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        public CachedItem(T item, TimeSpan expiry)
        {
            this.Item = item;
            this.Expiry = DateTime.Now + expiry;
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// Gets the expiry.
        /// </summary>
        public DateTime Expiry { get; private set; }

        /// <summary>
        /// Gets a value indicating whether is expired.
        /// </summary>
        public bool IsExpired
        {
            get
            {
                return DateTime.Now > this.Expiry;
            }
        }
    }
}