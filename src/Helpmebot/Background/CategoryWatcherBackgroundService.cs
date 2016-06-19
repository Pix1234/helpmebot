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
    using System.Linq;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.Background.Interfaces;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;

    using NHibernate;
    using NHibernate.Criterion;

    using Priority_Queue;

    /// <summary>
    /// The category watcher controller.
    /// </summary>
    public class CategoryWatcherBackgroundService : ICategoryWatcherBackgroundService
    {
        private readonly IIrcClient ircClient;

        private readonly ISession session;

        private readonly ILogger logger;

        private readonly Thread schedulerThread;

        private bool stopping;

        private SimplePriorityQueue<ActiveCategoryWatcher> schedule;

        private Dictionary<CategoryWatcher, ActiveCategoryWatcher> activeLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryWatcherBackgroundService"/> class.
        /// </summary>
        /// <param name="ircClient">
        /// The IRC Client.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger
        /// </param>
        public CategoryWatcherBackgroundService(IIrcClient ircClient, ISession session, ILogger logger)
        {
            this.ircClient = ircClient;
            this.session = session;
            this.logger = logger;

            this.schedulerThread = new Thread(this.Scheduler);
            this.schedule = new SimplePriorityQueue<ActiveCategoryWatcher>();
            this.activeLookup = new Dictionary<CategoryWatcher, ActiveCategoryWatcher>();
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
        /// The initialise watchers.
        /// </summary>
        private void InitialiseWatchers()
        {
            List<CategoryWatcher> categoryWatchers =
                this.session.CreateCriteria<CategoryWatcher>()
                    .Add(Restrictions.Eq("Enabled", true))
                    .List<CategoryWatcher>().ToList();

            foreach (var watcher in categoryWatchers)
            {
                this.EnableWatcher(watcher);
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
                var firstCatWatcher = this.schedule.First;
                var remaining = firstCatWatcher.NextTrigger - DateTime.Now;

                if (remaining.TotalMilliseconds <= 0)
                {
                    // Update time!
                    var categoryWatcher = firstCatWatcher.Watcher;

                    // do the update
                    this.PerformCategoryUpdate(categoryWatcher);

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

        private void PerformCategoryUpdate(CategoryWatcher categoryWatcher)
        {
            this.ircClient.SendMessage(categoryWatcher.Channel.Name, "CW update - " + categoryWatcher.Keyword);
        }
    }
}