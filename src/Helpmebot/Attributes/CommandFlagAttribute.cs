// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandFlagAttribute.cs" company="Helpmebot Development Team">
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
//   The command flag attribute.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Attributes
{
    using System;

    /// <summary>
    /// The command flag attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CommandFlagAttribute : Attribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandFlagAttribute"/> class.
        /// </summary>
        /// <param name="flag">
        /// The flag.
        /// </param>
        public CommandFlagAttribute(string flag)
        {
            this.Flag = flag;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or flag.
        /// </summary>
        public string Flag { get; private set; }

        #endregion
    }
}