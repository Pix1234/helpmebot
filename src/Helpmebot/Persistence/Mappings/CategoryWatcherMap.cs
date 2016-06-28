// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcherMap.cs" company="Helpmebot Development Team">
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
//   Defines the CategoryWatcherMap type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;

    using Helpmebot.Model;

    public class CategoryWatcherMap : ClassMap<CategoryWatcher>
    {
        public CategoryWatcherMap()
        {
            this.Table("categorywatcher");
            this.Id(x => x.Id, "id").GeneratedBy.GuidComb();
            this.References(x => x.Channel, "channel");
            this.References(x => x.MediaWikiSite, "mediawikisite");
            this.Map(x => x.Category, "category");
            this.Map(x => x.Priority, "priority");
            this.Map(x => x.Keyword, "keyword");
            this.Map(x => x.SleepTime, "sleeptime");
            this.Map(x => x.ShowWikiLink, "showwikilink");
            this.Map(x => x.ShowShortUrl, "showshorturl");
            this.Map(x => x.ShowWaitTime, "showwaittime");
            this.Map(x => x.MinimumWaitTime, "minwaittime");
            this.Map(x => x.Delta, "delta");
            this.Map(x => x.Enabled, "enabled");
            this.Map(x => x.ItemSingular, "itemSingular");
            this.Map(x => x.ItemPlural, "itemPlural");
            this.Map(x => x.ItemAction, "itemAction");
        }
    }
}