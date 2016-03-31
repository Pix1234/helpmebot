﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandInvocationAttribute.cs" company="Helpmebot Development Team">
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
//   The command invocation attribute.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Attributes
{
    using System;

    /// <summary>
    /// The command invocation attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandInvocationAttribute : Attribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandInvocationAttribute"/> class.
        /// </summary>
        /// <param name="commandName">
        /// The command name.
        /// </param>
        public CommandInvocationAttribute(string commandName)
        {
            this.CommandName = commandName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string CommandName { get; private set; }

        #endregion
    }
}