// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcher.cs" company="Helpmebot Development Team">
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
//   Defines the CategoryWatcher type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Model
{
    using Helpmebot.Persistence;

    /// <summary>
    /// The category watcher.
    /// </summary>
    public class CategoryWatcher : GuidEntityBase
    {
        public CategoryWatcher()
        {
            this.Priority = 1;
            this.SleepTime = 20;
            this.ShowWikiLink = true;
            this.ShowShortUrl = true;
            this.MinimumWaitTime = 60;
        }

        public virtual string Category { get; set; }

        public virtual Channel Channel { get; set; }

        public virtual bool Delta { get; set; }

        public virtual bool Enabled { get; set; }

        public virtual string Keyword { get; set; }

        public virtual MediaWikiSite MediaWikiSite { get; set; }

        public virtual int MinimumWaitTime { get; set; }

        public virtual int Priority { get; set; }

        public virtual bool ShowShortUrl { get; set; }

        public virtual bool ShowWaitTime { get; set; }

        public virtual bool ShowWikiLink { get; set; }

        public virtual int SleepTime { get; set; }


    }
}