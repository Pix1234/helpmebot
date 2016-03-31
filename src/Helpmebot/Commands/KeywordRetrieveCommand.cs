// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeywordRetrieveCommand.cs" company="Helpmebot Development Team">
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
//   The keyword retrieve command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    using NHibernate;

    /// <summary>
    /// The keyword retrieve command.
    /// </summary> 
    [CommandFlag(Model.Flag.Standard)]
    public class KeywordRetrieveCommand : CommandBase
    {
        #region Fields

        /// <summary>
        /// The keyword.
        /// </summary>
        private readonly Keyword keyword;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="KeywordRetrieveCommand"/> class.
        /// </summary>
        /// <param name="commandSource">
        /// The command source.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        /// <param name="commandServiceHelper">
        /// The command Service Helper.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        public KeywordRetrieveCommand(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            ILogger logger, 
            ISession databaseSession, 
            ICommandServiceHelper commandServiceHelper,
            Keyword keyword)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
            this.keyword = keyword;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();

            dict.Add("username", this.User.Username);
            dict.Add("nickname", this.User.Nickname);
            dict.Add("hostname", this.User.Hostname);
            dict.Add("channel", this.CommandSource);

            var flagsForUser = this.CommandServiceHelper.UserFlagService.GetFlagsForUser(this.User)
                .Implode(string.Empty);

            dict.Add("flags", flagsForUser);
            dict.Add("accessLevel", flagsForUser);

            var args = this.Arguments.ToArray();

            for (int i = 0; i < args.Length; i++)
            {
                dict.Add(i.ToString(CultureInfo.InvariantCulture), args[i]);
                dict.Add(i + "*", string.Join(" ", args, i, args.Length - i));
            }

            var wordResponse = this.keyword.Response.FormatWith(dict);

            yield return
                new CommandResponse
                    {
                        Message = wordResponse,
                        ClientToClientProtocol = this.keyword.Action ? "ACTION" : null
                    };
        }

        #endregion
    }
}