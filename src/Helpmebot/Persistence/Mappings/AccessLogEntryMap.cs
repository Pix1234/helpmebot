// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessLogEntryMap.cs" company="Helpmebot Development Team">
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
//   The access log entry map.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;

    using Helpmebot.Model;

    /// <summary>
    /// The access log entry map.
    /// </summary>
    public class AccessLogEntryMap : ClassMap<AccessLogEntry>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="AccessLogEntryMap"/> class.
        /// </summary>
        public AccessLogEntryMap()
        {
            this.Table("accesslog");
            this.Id(x => x.Id, "al_id");
            this.Map(x => x.Account, "account");
            this.Map(x => x.Arguments, "al_args");
            this.Map(x => x.Channel, "al_channel");
            this.Map(x => x.Command, "al_class");
            this.Map(x => x.ExecutionAllowed, "al_allowed");
            this.Map(x => x.RequiredFlag, "al_reqaccesslevel");
            this.Map(x => x.Timestamp, "al_date");
            this.Map(x => x.UserFlags, "al_accesslevel");
            this.Map(x => x.UserIdentifier, "al_nuh");
        }

        #endregion
    }
}