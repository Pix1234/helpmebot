// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportHelper.cs" company="Helpmebot Development Team">
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
//   Defines the SupportHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC
{
    using System.Collections.Generic;

    using Castle.Core.Logging;

    /// <summary>
    /// The support helper.
    /// </summary>
    public class SupportHelper
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportHelper"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public SupportHelper(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// The handle prefix message.
        /// </summary>
        /// <param name="prefixMessage">
        /// The prefix message.
        /// </param>
        /// <param name="prefixes">
        /// The prefix dictionary to modify
        /// </param>
        public void HandlePrefixMessageSupport(string prefixMessage, IDictionary<string, string> prefixes)
        {
            //// PREFIX=(Yqaohv)!~&@%+
            var strings = prefixMessage.Split('(', ')');
            var modes = strings[1];
            var symbols = strings[2];
            if (modes.Length != symbols.Length)
            {
                this.logger.ErrorFormat("RPL_ISUPPORT PREFIX not valid: {0}", prefixMessage);
                return;
            }

            for (int i = 0; i < modes.Length; i++)
            {
                prefixes.Add(modes.Substring(i, 1), symbols.Substring(i, 1));
            }
        }

        /// <summary>
        /// The handle status message.
        /// </summary>
        /// <param name="statusMessage">
        /// The status message.
        /// </param>
        /// <param name="supportedDestinationFlags">
        /// the supported destination flags list to modify
        /// </param>
        public void HandleStatusMessageSupport(string statusMessage, IList<string> supportedDestinationFlags)
        {
            //// STATUSMSG=@+
            var strings = statusMessage.Split('=');
            var modes = strings[1];

            for (int i = 0; i < modes.Length; i++)
            {
                supportedDestinationFlags.Add(modes.Substring(i, 1));
            }
        }
    }
}
