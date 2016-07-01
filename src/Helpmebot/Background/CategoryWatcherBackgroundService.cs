// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcherBackgroundService.cs" company="Helpmebot Development Team">
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
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Background
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.Background.Interfaces;
    using Helpmebot.Commands.CategoryWatcher;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;
    using NHibernate.Criterion;

    using Priority_Queue;

    /// <summary>
    /// The category watcher controller.
    /// </summary>
    public class CategoryWatcherBackgroundService : ICategoryWatcherBackgroundService
    {
        private readonly IIrcClient ircClient;

        private readonly ISession databaseSession;

        private readonly ILogger logger;

        private readonly IIgnoredPagesRepository ignoredPagesRepository;

        private readonly Thread schedulerThread;

        private readonly IUrlShorteningService urlShorteningService;

        private readonly ICommandParser commandParser;

        private readonly SimplePriorityQueue<ActiveCategoryWatcher> schedule;

        private readonly Dictionary<CategoryWatcher, ActiveCategoryWatcher> activeLookup;

        private readonly Dictionary<string, Dictionary<string, CategoryWatcher>> channelLookup;

        private bool stopping;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryWatcherBackgroundService"/> class.
        /// </summary>
        /// <param name="ircClient">
        /// The IRC Client.
        /// </param>
        /// <param name="databaseSession">
        /// The databaseSession.
        /// </param>
        /// <param name="logger">
        /// The logger
        /// </param>
        /// <param name="ignoredPagesRepository">
        /// The ignored Pages Repository.
        /// </param>
        /// <param name="urlShorteningService">
        /// The URL Shortening service
        /// </param>
        /// <param name="commandParser">
        /// The command parser
        /// </param>
        public CategoryWatcherBackgroundService(
            IIrcClient ircClient,
            ISession databaseSession,
            ILogger logger,
            IIgnoredPagesRepository ignoredPagesRepository,
            IUrlShorteningService urlShorteningService,
            ICommandParser commandParser)
        {
            this.ircClient = ircClient;
            this.databaseSession = databaseSession;
            this.logger = logger;
            this.ignoredPagesRepository = ignoredPagesRepository;
            this.urlShorteningService = urlShorteningService;
            this.commandParser = commandParser;

            this.schedulerThread = new Thread(this.Scheduler);
            this.schedule = new SimplePriorityQueue<ActiveCategoryWatcher>();

            this.activeLookup = new Dictionary<CategoryWatcher, ActiveCategoryWatcher>();
            this.channelLookup = new Dictionary<string, Dictionary<string, CategoryWatcher>>();
        }

        /// <summary>
        /// The disable watcher.
        /// </summary>
        /// <param name="watcher">
        /// The watcher.
        /// </param>
        public void DisableWatcher(CategoryWatcher watcher)
        {
            ActiveCategoryWatcher activeWatcher;
            if (this.activeLookup.TryGetValue(watcher, out activeWatcher))
            {
                this.schedule.Remove(activeWatcher);
                this.activeLookup.Remove(watcher);
                this.channelLookup[watcher.Channel.Name].Remove(watcher.Keyword);
            }
        }

        /// <summary>
        /// The enable watcher.
        /// </summary>
        /// <param name="watcher">
        /// The watcher.
        /// </param>
        public void EnableWatcher(CategoryWatcher watcher)
        {
            ActiveCategoryWatcher activeWatcher;
            if (!this.activeLookup.TryGetValue(watcher, out activeWatcher))
            {
                var activeCategoryWatcher = new ActiveCategoryWatcher(watcher);
                this.activeLookup.Add(watcher, activeCategoryWatcher);

                if (!this.channelLookup.ContainsKey(watcher.Channel.Name))
                {
                    this.channelLookup.Add(watcher.Channel.Name, new Dictionary<string, CategoryWatcher>());
                }
                this.channelLookup[watcher.Channel.Name].Add(watcher.Keyword, watcher);

                this.schedule.Enqueue(activeCategoryWatcher, activeCategoryWatcher.NextTrigger.Ticks);
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            this.logger.Info("Starting background service");
            this.schedulerThread.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            this.logger.Info("Stopping background service");
            this.stopping = true;
            this.schedulerThread.Interrupt();
            this.schedulerThread.Join();
            this.logger.Info("Background task stopped");
        }

        /// <summary>
        /// Triggers an update for a category watcher in a specific channel
        /// </summary>
        /// <param name="keyword">The keyword identifying the category watcher</param>
        /// <param name="channel">The channel identifying the category watcher</param>
        public void TriggerCategoryWatcherUpdate(string keyword, Channel channel)
        {
            Dictionary<string, CategoryWatcher> channelWatchers;
            if (!this.channelLookup.TryGetValue(channel.Name, out channelWatchers))
            {
                return;
            }

            CategoryWatcher watcher;
            if (channelWatchers.TryGetValue(keyword, out watcher))
            {
                this.PerformCategoryUpdate(watcher, false);
            }
        }

        public IDictionary<string, CategoryWatcher> GetWatchersForChannel(Channel channel)
        {
            Dictionary<string, CategoryWatcher> lookupResult;
            if (this.channelLookup.TryGetValue(channel.Name, out lookupResult))
            {
                return new ReadOnlyDictionary<string, CategoryWatcher>(lookupResult);
            }

            return new ReadOnlyDictionary<string, CategoryWatcher>(new Dictionary<string, CategoryWatcher>());
        }

        /// <summary>
        /// The initialise watchers.
        /// </summary>
        private void InitialiseWatchers()
        {
            List<CategoryWatcher> categoryWatchers =
                this.databaseSession.CreateCriteria<CategoryWatcher>()
                    .Add(Restrictions.Eq("Enabled", true))
                    .List<CategoryWatcher>().ToList();

            foreach (var watcher in categoryWatchers)
            {
                this.EnableWatcher(watcher);
                
                this.commandParser.RegisterCommand(watcher.Keyword.ToLower(CultureInfo.InvariantCulture), typeof(CategoryWatcherForceCommand), watcher.Channel.Name);
            }
        }

        /// <summary>
        /// The scheduler task. This runs the show.
        /// </summary>
        private void Scheduler()
        {
            this.InitialiseWatchers();

            this.ircClient.WaitOnRegistration();

            while (true)
            {
                var firstCatWatcher = this.schedule.FirstOrDefault();
                if (firstCatWatcher == null)
                {
                    // No category watchers enabled, wait a while
                    Thread.Sleep(5000);
                    continue;
                }

                var remaining = firstCatWatcher.NextTrigger - DateTime.Now;

                if (remaining.TotalMilliseconds <= 0)
                {
                    // Update time!
                    var categoryWatcher = firstCatWatcher.Watcher;

                    try
                    {
                        // do the update
                        this.PerformCategoryUpdate(categoryWatcher, true);
                    }
                    catch (Exception ex)
                    {
                        this.logger.ErrorFormat(
                            ex,
                            "Error encountered doing automated update on {0} in channel {1}",
                            categoryWatcher.Keyword,
                            categoryWatcher.Channel.Name);
                    }

                    // reschedule for the next slot
                    firstCatWatcher.NextTrigger = firstCatWatcher.NextTrigger.AddMinutes(categoryWatcher.SleepTime);
                    this.schedule.UpdatePriority(firstCatWatcher, firstCatWatcher.NextTrigger.Ticks);
                }
                else
                {
                    int sleepTime;

                    var millisecWait = remaining.TotalMilliseconds / 2;

                    if (millisecWait > int.MaxValue)
                    {
                        sleepTime = int.MaxValue;
                    }
                    else
                    {
                        sleepTime = Math.Max((int)millisecWait, 250);
                    }

                    Thread.Sleep(sleepTime);
                }

                if (this.stopping)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// The perform category update.
        /// </summary>
        /// <param name="categoryWatcher">
        /// The category watcher.
        /// </param>
        /// <param name="scheduled">
        /// Whether this update is caused by a scheduled update or not
        /// </param>
        private void PerformCategoryUpdate(CategoryWatcher categoryWatcher, bool scheduled)
        {
            var transaction = this.databaseSession.BeginTransaction();
            try
            {
                var watcherPageList = this.GetWatcherPageList(categoryWatcher.Category, categoryWatcher.MediaWikiSite);

                IEnumerable<CategoryWatcherItem> newStuff;
                var categoryWatcherItems = this.SynchroniseDatabase(categoryWatcher, watcherPageList, out newStuff);

                // reload the channel, since it's config could have changed.
                this.databaseSession.Refresh(categoryWatcher.Channel);
                this.databaseSession.Refresh(categoryWatcher);
                
                // Override the full list if this CW is setup to only show the delta, and it's a scheduled update
                if (scheduled && categoryWatcher.Delta)
                {
                    categoryWatcherItems = newStuff;
                }

                if (!categoryWatcher.Channel.IsSilenced)
                {
                    var message = this.CompileMessage(categoryWatcher, categoryWatcherItems);

                    this.ircClient.SendMessage(categoryWatcher.Channel.Name, message);
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private string CompileMessage(CategoryWatcher categoryWatcher, IEnumerable<CategoryWatcherItem> categoryWatcherItems)
        {
            TimeSpan minimumWaitTime = new TimeSpan(0, categoryWatcher.MinimumWaitTime, 0);
            
            var watcherItems = categoryWatcherItems as IList<CategoryWatcherItem> ?? categoryWatcherItems.ToList();
            
            if (watcherItems.Any())
            {
                var messageFormat = this.GetMessageFormat(categoryWatcher);

                var itemList =
                    watcherItems.Select(x => this.FormatItem(categoryWatcher, x, minimumWaitTime, messageFormat)).ToList();

                var pageList = string.Join(", ", itemList);

                var pluralString = itemList.Count == 1 ? categoryWatcher.ItemSingular : categoryWatcher.ItemPlural;

                var totalCount = categoryWatcher.MediaWikiSite.GetCategorySize(categoryWatcher.Category);

                return string.Format(
                    "{0} {1}{2}: {3}",
                    totalCount,
                    pluralString,
                    string.Format(" " + categoryWatcher.ItemAction, categoryWatcher.Category).TrimEnd(' '),
                    pageList);
            }
            else
            {
                return string.Format("0 {0} {1}.", categoryWatcher.ItemPlural, categoryWatcher.ItemAction);
            }
        }

        private string FormatItem(
            CategoryWatcher categoryWatcher,
            CategoryWatcherItem x,
            TimeSpan minimumWaitTime,
            string messageFormat)
        {
            var shortUrl = string.Empty;

            if (categoryWatcher.ShowShortUrl)
            {
                var longUrl = string.Format(categoryWatcher.MediaWikiSite.ArticlePath, x.Title);
                shortUrl = this.urlShorteningService.Shorten(longUrl);
            }

            var waitTime = string.Empty;
            var waiting = DateTime.Now - x.Timestamp;
            if (categoryWatcher.ShowWaitTime && waiting > minimumWaitTime)
            {
                waitTime = string.Format(
                    "(waiting {0})",
                    waiting.ToString((waiting.Days > 0 ? "d'd '" : string.Empty) + @"hh\:mm\:ss"));
            }

            return string.Format(messageFormat, x.Title, shortUrl, waitTime);
        }

        private string GetMessageFormat(CategoryWatcher categoryWatcher)
        {
            StringBuilder formatBuilder = new StringBuilder();
            bool noTrim = false;
            if (categoryWatcher.ShowWikiLink)
            {
                formatBuilder.Append("[[{0}]] ");
            }

            if (categoryWatcher.ShowShortUrl)
            {
                formatBuilder.Append("{1} ");
                noTrim = true;
            }

            if (categoryWatcher.ShowWaitTime)
            {
                formatBuilder.Append("{2}");
                noTrim = false;
            }

            var messageFormat = formatBuilder.ToString();

            if (!noTrim)
            {
                messageFormat = messageFormat.Trim();
            }

            return messageFormat;
        }

        /// <summary>
        /// Synchronises the database cache of pending category items with the API list
        /// </summary>
        /// <param name="watcher">
        /// The watcher.
        /// </param>
        /// <param name="watcherPageList">
        /// The watcher page list.
        /// </param>
        private IEnumerable<CategoryWatcherItem> SynchroniseDatabase(CategoryWatcher watcher, IEnumerable<string> watcherPageList, out IEnumerable<CategoryWatcherItem> newStuff)
        {
            var categoryWatcherItems =
                this.databaseSession.CreateCriteria<CategoryWatcherItem>()
                    .Add(Restrictions.Eq("CategoryWatcher", watcher))
                    .List<CategoryWatcherItem>()
                    .ToDictionary(x => x.Title);

            List<string> toAdd, toRemove;
            var changes = categoryWatcherItems.Keys.ToList().Delta(watcherPageList.ToList(), out toAdd, out toRemove);

            var newStuffList = new List<CategoryWatcherItem>();
            newStuff = newStuffList;

            if (changes == 0)
            {
                // nothing to do, so carry on
                return categoryWatcherItems.Values;
            }
            
            // remove first so the database can reuse allocated space
            foreach (var page in toRemove)
            {
                this.databaseSession.Delete(categoryWatcherItems[page]);
                categoryWatcherItems.Remove(page);
            }

            var touched = watcher.MediaWikiSite.GetTouchedTime(toAdd);
            foreach (var page in toAdd)
            {
                DateTime lastTouched;
                if (!touched.TryGetValue(page, out lastTouched))
                {
                    lastTouched = DateTime.Now;
                }

                var item = new CategoryWatcherItem { CategoryWatcher = watcher, Title = page, Timestamp = lastTouched };
                
                this.databaseSession.Save(item);

                categoryWatcherItems.Add(page, item);
                newStuffList.Add(item);
            }
            
            return categoryWatcherItems.Values;
        }
        
        /// <summary>
        /// Gets the valid (aka non-blacklisted pages) for the watcher specified
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="mediaWikiSite">
        /// The media Wiki Site.
        /// </param>
        /// <returns>
        /// List of page titles, or null on error
        /// </returns>
        private IEnumerable<string> GetWatcherPageList(string category, MediaWikiSite mediaWikiSite)
        {
            this.logger.Info("Getting items in category " + category);

            IEnumerable<string> pages;
            try
            {
                pages = mediaWikiSite.GetPagesInCategory(category);
            }
            catch (Exception ex)
            {
                this.logger.Error("Error contacting API (" + mediaWikiSite.Api + ") ", ex);
                return null;
            }
            
            pages = this.RemoveBlacklistedItems(pages).ToList();

            return pages;
        }

        /// <summary>
        /// The remove blacklisted items.
        /// </summary>
        /// <param name="pageList">
        /// The page list.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        private IEnumerable<string> RemoveBlacklistedItems(IEnumerable<string> pageList)
        {
            var ignoredPages = this.ignoredPagesRepository.GetIgnoredPages().ToList();

            return pageList.Where(x => !ignoredPages.Contains(x));
        }
    }
}