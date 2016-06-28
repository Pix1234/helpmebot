// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcherItemMap.cs" company="Helpmebot Development Team">
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
//   Defines the CategoryWatcherItemMap type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;

    using Helpmebot.Model;

    public class CategoryWatcherItemMap : ClassMap<CategoryWatcherItem>
    {
        public CategoryWatcherItemMap()
        {
            this.Table("categorywatcher_items");
            this.Id(x => x.Id, "id").GeneratedBy.GuidComb();
            this.References(x => x.CategoryWatcher, "categorywatcher");
            this.Map(x => x.Title, "title");
            this.Map(x => x.Timestamp, "touched");
        }
    }
}