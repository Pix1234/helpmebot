// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserFlagService.cs" company="Helpmebot Development Team">
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
//   Defines the UserFlagService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Internal;
    using Castle.Core.Logging;

    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;
    using NHibernate.Linq;

    /// <summary>
    /// The user flag service.
    /// </summary>
    public class UserFlagService : IUserFlagService
    {
        #region Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The flag group users.
        /// </summary>
        private IList<FlagGroupUser> flagGroupUsers;

        /// <summary>
        /// The flag group channels.
        /// </summary>
        private IList<FlagGroupChannel> flagGroupChannels;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="UserFlagService"/> class.
        /// </summary>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public UserFlagService(ISession databaseSession, ILogger logger)
        {
            this.logger = logger;
            this.DatabaseSession = databaseSession;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the database session.
        /// </summary>
        public ISession DatabaseSession { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The flags for user.
        /// </summary>
        /// <param name="flagGroups">
        /// The flag groups.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        public static IEnumerable<string> GetFlagsFromGroups(IEnumerable<FlagGroup> flagGroups)
        {
            var flags = new HashSet<string>();

            var flagGroupList = flagGroups.ToList();

            foreach (var flag in flagGroupList.Where(x => !x.DenyGroup))
            {
                flag.Flags.Select(x => x.Flag).ForEach(x => flags.Add(x));
            }

            foreach (var flag in flagGroupList.Where(x => x.DenyGroup))
            {
                flag.Flags.Select(x => x.Flag).ForEach(x => flags.Remove(x));
            }

            return flags;
        }

        /// <summary>
        /// The get flags for user.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        public IEnumerable<string> GetFlagsForUser(IUser user)
        {
            if (this.flagGroupUsers == null)
            {
                this.logger.DebugFormat("Retrieving flag groups for user {1} from database", user);
                this.flagGroupUsers = this.DatabaseSession.QueryOver<FlagGroupUser>().List();
            }

            var account = (user.Account ?? string.Empty).ToLower();
            var nickname = (user.Nickname ?? string.Empty).ToLower();
            var username = (user.Username ?? string.Empty).ToLower();
            var hostname = (user.Hostname ?? string.Empty).ToLower();

            var flagGroups = (from flagGroupUser in this.flagGroupUsers
                              where
                                  flagGroupUser.AccountRegex.Match(account).Success
                                  && flagGroupUser.NicknameRegex.Match(nickname).Success
                                  && flagGroupUser.UsernameRegex.Match(username).Success
                                  && flagGroupUser.HostnameRegex.Match(hostname).Success
                              select flagGroupUser.FlagGroup).ToList();

            this.logger.DebugFormat("Retrieved {0} flag groups for user {1}", flagGroups.Count, user);

            var flagsForUser = GetFlagsFromGroups(flagGroups).ToList();

            this.logger.InfoFormat("Found flags for user {1}: {0}", flagsForUser.Implode(), user);
            
            return flagsForUser;
        }

        /// <summary>
        /// The get flags for channel.
        /// </summary>
        /// <param name="channel">
        /// The channel name.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        public IEnumerable<string> GetFlagsForChannel(Channel channel)
        {
            if (channel == null)
            {
                return new List<string>();
            }

            if (this.flagGroupChannels == null)
            {
                this.logger.DebugFormat("Retrieving flag groups for channel {1} from database", channel);
                this.flagGroupChannels = this.DatabaseSession.QueryOver<FlagGroupChannel>().List();
            }

            var groups = this.flagGroupChannels.Where(x => x.Channel.Equals(channel)).Select(x => x.FlagGroup).ToList();

            this.logger.DebugFormat("Retrieved {0} flag groups for channel {1}", groups.Count, channel);

            var flagsForChannel = GetFlagsFromGroups(groups).ToList();

            this.logger.InfoFormat("Found flags for channel {1}: {0}", flagsForChannel.Implode(), channel);

            return flagsForChannel;
        }

        /// <summary>
        /// The invalidate cache.
        /// </summary>
        public void InvalidateCache()
        {
            this.flagGroupUsers = null;
            this.flagGroupChannels = null;
        }

        #endregion
    }
}