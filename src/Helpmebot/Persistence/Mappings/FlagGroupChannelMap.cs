// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupChannelMap.cs" company="Helpmebot Development Team">
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
//   The flag group channel map.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;

    using Helpmebot.Model;

    /// <summary>
    /// The flag group channel map.
    /// </summary>
    public class FlagGroupChannelMap : ClassMap<FlagGroupChannel>
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="FlagGroupChannelMap"/> class.
        /// </summary>
        public FlagGroupChannelMap()
        {
            this.Table("flaggroup_channel");
            this.Id(x => x.Id, "id").GeneratedBy.GuidComb();
            this.References(x => x.FlagGroup, "flaggroup_id");
            this.References(x => x.Channel, "channel_id");
            this.Map(x => x.IsProtected, "protected");
        }
    }
}