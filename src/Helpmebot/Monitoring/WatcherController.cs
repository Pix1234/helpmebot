// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WatcherController.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Monitoring
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using Castle.Core.Logging;

    using Helpmebot.Commands.CategoryWatcher;
    using Helpmebot.Configuration.XmlSections.Interfaces;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using MySql.Data.MySqlClient;

    using NHibernate;

    /// <summary>
    ///     Controls instances of CategoryWatchers for the bot
    /// </summary>
    internal class WatcherController
    {
        #region Static Fields

        /// <summary>
        ///     The _instance.
        /// </summary>
        private static WatcherController instance;

        #endregion

        #region Fields

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The IRC client.
        /// </summary>
        private readonly IIrcClient ircClient;

        /// <summary>
        ///     The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        ///     The url shortening service.
        /// </summary>
        private readonly IUrlShorteningService urlShorteningService;

        /// <summary>
        ///     The watchers.
        /// </summary>
        private readonly Dictionary<string, CategoryWatcher> watchers;

        /// <summary>
        /// The legacy database.
        /// </summary>
        private readonly ILegacyDatabase legacyDatabase;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="WatcherController"/> class.
        /// </summary>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        /// <param name="urlShorteningService">
        /// The url Shortening Service.
        /// </param>
        /// <param name="watchedCategoryRepository">
        /// The watched Category Repository.
        /// </param>
        /// <param name="ignoredPagesRepository">
        /// The ignored Pages Repository.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="ircClient">
        /// The IRC Client.
        /// </param>
        /// <param name="legacyDatabase">
        /// The legacy Database.
        /// </param>
        /// <param name="commandParser">
        /// The command Parser.
        /// </param>
        /// <param name="coreConfiguration">
        /// The core Configuration.
        /// </param>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        protected WatcherController(
            IMessageService messageService, 
            IUrlShorteningService urlShorteningService, 
            IWatchedCategoryRepository watchedCategoryRepository,
            IIgnoredPagesRepository ignoredPagesRepository,
            ILogger logger,
            IIrcClient ircClient,
            ILegacyDatabase legacyDatabase,
            ICommandParser commandParser,
            ICoreConfiguration coreConfiguration,
            ISession databaseSession)
        {
            this.messageService = messageService;
            this.urlShorteningService = urlShorteningService;
            this.watchers = new Dictionary<string, CategoryWatcher>();
            this.logger = logger;
            this.ircClient = ircClient;

            foreach (WatchedCategory item in watchedCategoryRepository.Get())
            {
                var categoryWatcher = new CategoryWatcher(
                    item,
                    ignoredPagesRepository,
                    coreConfiguration,
                    databaseSession,
                    logger.CreateChildLogger("CategoryWatcher[" + item.Keyword + "]"));
                this.watchers.Add(item.Keyword, categoryWatcher);
                categoryWatcher.CategoryHasItemsEvent += this.CategoryHasItemsEvent;

                commandParser.RegisterCommand(item.Keyword, typeof(CategoryWatcherForceCommand));
            }

            this.legacyDatabase = legacyDatabase;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The instance.
        /// </summary>
        /// <returns>
        ///     The <see cref="WatcherController" />.
        /// </returns>
        public static WatcherController Instance()
        {
            if (instance == null)
            {
                // FIXME: ServiceLocator - ALL THE THINGS!
                var ms = ServiceLocator.Current.GetInstance<IMessageService>();
                var ss = ServiceLocator.Current.GetInstance<IUrlShorteningService>();
                var wcrepo = ServiceLocator.Current.GetInstance<IWatchedCategoryRepository>();
                var iprepo = ServiceLocator.Current.GetInstance<IIgnoredPagesRepository>();
                var logger = ServiceLocator.Current.GetInstance<ILogger>();
                var irc = ServiceLocator.Current.GetInstance<IIrcClient>();
                var legacyDb = ServiceLocator.Current.GetInstance<ILegacyDatabase>();
                var coreConfiguration = ServiceLocator.Current.GetInstance<ICoreConfiguration>();
                var databaseSession = ServiceLocator.Current.GetInstance<ISession>();
                legacyDb.Connect();

                var parser = ServiceLocator.Current.GetInstance<ICommandParser>();

                instance = new WatcherController(
                    ms,
                    ss,
                    wcrepo,
                    iprepo,
                    logger,
                    irc,
                    legacyDb,
                    parser,
                    coreConfiguration,
                    databaseSession);
            }

            return instance;
        }

        /// <summary>
        /// Forces the update.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <returns>
        /// the compile
        /// </returns>
        public string ForceUpdate(string key, string destination)
        {
            this.logger.InfoFormat("Forcing update for {0} at {1}.", key, destination);

            CategoryWatcher cw;
            if (this.watchers.TryGetValue(key, out cw))
            {
                List<string> items = cw.DoCategoryCheck().ToList();
                this.UpdateDatabaseTable(items, key);
                return this.CompileMessage(items, key, destination, true);
            }

            return null;
        }

        /// <summary>
        ///     Gets the keywords.
        /// </summary>
        /// <returns>
        ///     The list of keywords
        /// </returns>
        public Dictionary<string, CategoryWatcher>.KeyCollection GetKeywords()
        {
            return this.watchers.Keys;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The update database table.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        private void UpdateDatabaseTable(IEnumerable<string> items, string keyword)
        {
            // MIGRATED
        }

        /// <summary>
        /// The category has items event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CategoryHasItemsEvent(object sender, CategoryHasItemsEventArgs e)
        {
            // migrated
        }

        /// <summary>
        /// The compile message.
        /// </summary>
        /// <param name="itemsEnumerable">
        /// The items enumerable.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="forceShowAll">
        /// The force show all.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string CompileMessage(
            IEnumerable<string> itemsEnumerable, 
            string keyword, 
            string destination, 
            bool forceShowAll)
        {
            //// keywordHasItems: 0: count, 1: plural word(s), 2: items in category
            //// keywordNoItems: 0: plural word(s)
            //// keywordPlural
            //// keywordSingular
            List<string> items = itemsEnumerable.ToList();

            bool showWaitTime = destination != string.Empty
                                && (LegacyConfig.Singleton()["showWaitTime", destination] == "true");

            TimeSpan minimumWaitTime;
            if (!TimeSpan.TryParse(LegacyConfig.Singleton()["minimumWaitTime", destination], out minimumWaitTime))
            {
                minimumWaitTime = new TimeSpan(0);
            }

            bool shortenUrls = destination != string.Empty
                               && (LegacyConfig.Singleton()["useShortUrlsInsteadOfWikilinks", destination] == "true");
            bool showDelta = destination != string.Empty
                             && (LegacyConfig.Singleton()["catWatcherShowDelta", destination] == "true");

            if (forceShowAll)
            {
                showDelta = false;
            }

            string message;

            if (items.Any())
            {
                string listString = string.Empty;
                var listSeparator = this.messageService.RetrieveMessage("listSeparator", destination, null);

                foreach (string item in items)
                {
                    // Display [[]]'ied name of the page which requests help
                    listString += "[[" + item + "]] ";

                    // Display an http URL to the page, if desired
                    if (shortenUrls)
                    {
                        string urlName = item.Replace(' ', '_');

                        // FIXME: configuration
                        string uriString = "http://enwp.org/" + HttpUtility.UrlEncode(urlName);
                        listString += this.urlShorteningService.Shorten(uriString);
                    }

                    if (showWaitTime)
                    {
                        var command =
                            new MySqlCommand(
                                "SELECT item_entrytime FROM categoryitems WHERE item_name = @name and item_keyword = @keyword;");

                        command.Parameters.AddWithValue("@name", item);
                        command.Parameters.AddWithValue("@keyword", keyword);

                        string insertDate = this.legacyDatabase.ExecuteScalarSelect(command);
                        DateTime realInsertDate;
                        if (!DateTime.TryParse(insertDate, out realInsertDate))
                        {
                            realInsertDate = DateTime.Now;
                        }

                        TimeSpan ts = DateTime.Now - realInsertDate;

                        if (ts >= minimumWaitTime)
                        {
                            string[] messageparams =
                                {
                                    ts.Hours.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), 
                                    ts.Minutes.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), 
                                    ts.Seconds.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'), 
                                    ts.TotalDays >= 1
                                        ? ((int)Math.Floor(ts.TotalDays)) + "d "
                                        : string.Empty
                                };
                            listString += this.messageService.RetrieveMessage(
                                "catWatcherWaiting", 
                                destination, 
                                messageparams);
                        }
                    }

                    // trailing space added as a hack because MediaWiki doesn't preserve the trailing space :(
                    listString += listSeparator + " ";
                }

                listString = listString.TrimEnd(' ', ',');
                string pluralString = items.Count() == 1
                                          ? this.messageService.RetrieveMessage(
                                              keyword + "Singular", 
                                              destination, 
                                              new[] { "keywordSingularDefault" })
                                          : this.messageService.RetrieveMessage(
                                              keyword + "Plural", 
                                              destination, 
                                              new[] { "keywordPluralDefault" });
                string[] messageParams =
                    {
                        items.Count().ToString(CultureInfo.InvariantCulture), pluralString, 
                        listString
                    };
                message = this.messageService.RetrieveMessage(
                    keyword + (showDelta ? "New" : string.Empty) + "HasItems", 
                    destination, 
                    messageParams);
            }
            else
            {
                string[] mp =
                    {
                        this.messageService.RetrieveMessage(
                            keyword + "Plural", 
                            destination, 
                            new[] { "keywordPluralDefault" })
                    };
                message = this.messageService.RetrieveMessage(keyword + "NoItems", destination, mp);
            }

            return message;
        }

        #endregion
    }
}