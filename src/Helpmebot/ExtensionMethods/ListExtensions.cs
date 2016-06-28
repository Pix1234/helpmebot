// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListExtensions.cs" company="Helpmebot Development Team">
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
//   Defines the ListExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ExtensionMethods
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Internal;

    /// <summary>
    /// The list extensions.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// The pop from front.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <typeparam name="T">
        /// the type of list
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static T PopFromFront<T>(this List<T> list)
        {
            var foo = list[0];
            list.RemoveAt(0);

            return foo;
        }

        /// <summary>
        /// The delta.
        /// </summary>
        /// <param name="oldList">
        /// The old list.
        /// </param>
        /// <param name="newList">
        /// The new list.
        /// </param>
        /// <param name="toAdd">
        /// The to add.
        /// </param>
        /// <param name="toRemove">
        /// The to remove.
        /// </param>
        /// <typeparam name="T">
        /// The type of list
        /// </typeparam>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int Delta<T>(
            this List<T> oldList,
            List<T> newList,
            out List<T> toAdd,
            out List<T> toRemove)
        {
            var toRemoveTmp = new List<T>(oldList);
            newList.ForEach(x => toRemoveTmp.Remove(x));

            var toAddTmp = new List<T>(newList);
            oldList.ForEach(x => toAddTmp.Remove(x));
            
            toAdd = toAddTmp.ToList();
            toRemove = toRemoveTmp.ToList();

            return toAdd.Count + toRemove.Count;
        }
    }
}
