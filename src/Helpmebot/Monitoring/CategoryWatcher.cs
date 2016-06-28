// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcher.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Monitoring
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.Configuration.XmlSections.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Threading;

    using NHibernate;

    /// <summary>
    ///     Category watcher thread
    /// </summary>
    public class CategoryWatcher : IThreadedSystem
    {
        /// <summary>
        ///     The category.
        /// </summary>
        private readonly string category;

        /// <summary>
        ///     The ignored pages repository.
        /// </summary>
        private readonly IIgnoredPagesRepository ignoredPagesRepository;

        /// <summary>
        ///     The key.
        /// </summary>
        private readonly string key;

        /// <summary>
        ///     The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        ///     The site.
        /// </summary>
        private readonly MediaWikiSite site;

        /// <summary>
        ///     The watched category.
        /// </summary>
        private readonly WatchedCategory watchedCategory;

        /// <summary>
        ///     The sleep time.
        /// </summary>
        private int sleepTime = 180;

        /// <summary>
        ///     The watcher thread.
        /// </summary>
        private Thread watcherThread;

        /// <summary>
        /// Initialises a new instance of the <see cref="CategoryWatcher"/> class.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="ignoredPagesRepository">
        /// The ignored Pages Repository.
        /// </param>
        /// <param name="coreConfiguration">
        /// The core Configuration.
        /// </param>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public CategoryWatcher(
            WatchedCategory category, 
            IIgnoredPagesRepository ignoredPagesRepository, 
            ICoreConfiguration coreConfiguration, 
            ISession databaseSession, 
            ILogger logger)
        {
            this.watchedCategory = category;

            this.logger = logger;

            this.site =
                databaseSession.QueryOver<MediaWikiSite>()
                    .Where(x => x.Name == coreConfiguration.DefaultMediaWikiSiteName)
                    .SingleOrDefault();

            this.category = category.Category;
            this.key = category.Keyword;
            this.sleepTime = category.SleepTime;

            this.logger.DebugFormat("Initial sleep time is {0}", this.sleepTime);

            this.RegisterInstance();

            this.watcherThread = new Thread(this.WatcherThreadMethod);
            this.watcherThread.Start();

            this.ignoredPagesRepository = ignoredPagesRepository;
        }

        /// <summary>
        ///     The category has items event.
        /// </summary>
        public event EventHandler<CategoryHasItemsEventArgs> CategoryHasItemsEvent;

        /// <summary>
        ///     The thread fatal error event.
        /// </summary>
        public event EventHandler ThreadFatalErrorEvent;

        /// <summary>
        ///     Gets or sets the time to sleep, in seconds.
        /// </summary>
        public int SleepTime
        {
            get
            {
                return this.sleepTime;
            }

            set
            {
                this.sleepTime = value;
                this.logger.InfoFormat("Restarting watcher due to time change (new time {0})...", value);
                this.watcherThread.Abort();
                Thread.Sleep(500);
                this.watcherThread = new Thread(this.WatcherThreadMethod);
                this.watcherThread.Start();
            }
        }

        /// <summary>
        ///     Gets the watched category.
        /// </summary>
        public WatchedCategory WatchedCategory
        {
            get
            {
                return this.watchedCategory;
            }
        }

        /// <summary>
        ///     The do category check.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerable" />.
        /// </returns>
        public IEnumerable<string> DoCategoryCheck()
        {
            // MIGRATED
            return new List<string>();
        }

        /// <summary>
        ///     The get thread status.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string[] GetThreadStatus()
        {
            string[] statuses = { this.key + " " + this.watcherThread.ThreadState };
            return statuses;
        }

        /// <summary>
        ///     The register instance.
        /// </summary>
        public void RegisterInstance()
        {
            ThreadList.GetInstance().Register(this);
        }

        /// <summary>
        ///     The stop.
        /// </summary>
        public void Stop()
        {
            this.logger.Info("Stopping Watcher Thread for " + this.category + " ...");
            this.watcherThread.Abort();
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.key;
        }

        /// <summary>
        ///     The watcher thread method.
        /// </summary>
        private void WatcherThreadMethod()
        {
            this.logger.Info("Starting category watcher for '" + this.key + "'...");
            try
            {
                while (true)
                {
                    this.logger.DebugFormat("Sleeping thread for {0} seconds", this.SleepTime);
                    int remaining = this.SleepTime * 1000;

                    // iteratively sleep (yuck) until we've got less than a second of our sleep remaining - sounds like a good enough tolerance for me.
                    while (remaining > 1000)
                    {
                        var millisecondsTimeout = remaining / 2;
                        var stopwatch = Stopwatch.StartNew();
                        Thread.Sleep(millisecondsTimeout);
                        stopwatch.Stop();

                        remaining -= (int)stopwatch.ElapsedMilliseconds;
                        this.logger.DebugFormat(
                            "Thread has woken after {0}ms, with {1} ms remaining", 
                            stopwatch.ElapsedMilliseconds, 
                            remaining);
                    }

                    this.logger.DebugFormat("Thread wakeup", this.SleepTime);

                    try
                    {
                        var categoryCheckResult = this.DoCategoryCheck();
                        IEnumerable<string> categoryResults = categoryCheckResult.ToList();

                        if (categoryResults.Any())
                        {
                            var onCategoryHasItemsEvent = this.CategoryHasItemsEvent;
                            if (onCategoryHasItemsEvent != null)
                            {
                                onCategoryHasItemsEvent(this, new CategoryHasItemsEventArgs(categoryResults, this.key));
                            }
                        }
                    }
                    catch (WebException e)
                    {
                        this.logger.Warn(e.Message, e);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                EventHandler temp = this.ThreadFatalErrorEvent;
                if (temp != null)
                {
                    temp(this, new EventArgs());
                }
            }

            this.logger.Warn("Category watcher for '" + this.key + "' died.");
        }
    }
}