// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedirectionResult.cs" company="Helpmebot Development Team">
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
//   The redirection result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands.CommandUtilities.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The redirection result.
    /// </summary>
    public class RedirectionResult
    {
        #region Fields

        /// <summary>
        /// The arguments.
        /// </summary>
        private readonly IEnumerable<string> arguments;

        /// <summary>
        /// The target.
        /// </summary>
        private readonly IEnumerable<string> target;

        /// <summary>
        /// The channel targets
        /// </summary>
        private readonly IEnumerable<string> channelTargets;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="RedirectionResult"/> class.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="channelTargets">
        /// The channel targets.
        /// </param>
        public RedirectionResult(IEnumerable<string> arguments, IEnumerable<string> target, IEnumerable<string> channelTargets)
        {
            this.arguments = arguments;
            this.target = target;
            this.channelTargets = channelTargets;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public IEnumerable<string> Arguments
        {
            get
            {
                return this.arguments;
            }
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        public IEnumerable<string> Target
        {
            get
            {
                return this.target;
            }
        }

        /// <summary>
        /// The channel targets
        /// </summary>
        public IEnumerable<string> ChannelTargets
        {
            get
            {
                return this.channelTargets;
            }
        }

        #endregion
    }
}