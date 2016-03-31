// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WikiLinkService.cs" company="Helpmebot Development Team">
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
//   The wiki link service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    using Castle.Core.Logging;

    using Helpmebot.Configuration.XmlSections.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.IRC.Events;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    /// <summary>
    /// The wiki link service.
    /// </summary>
    public class WikiLinkService : IWikiLinkService
    {
        #region Fields

        /// <summary>
        /// The database session.
        /// </summary>
        private readonly ISession databaseSession;

        /// <summary>
        /// The default interwiki prefix.
        /// </summary>
        private readonly InterwikiPrefix defaultInterwikiPrefix;

        /// <summary>
        /// The interwiki cache.
        /// </summary>
        private readonly IDictionary<string, InterwikiPrefix> interwikiCache;

        /// <summary>
        /// The last link cache.
        /// </summary>
        private readonly IDictionary<string, IEnumerable<string>> lastLinkCache;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="WikiLinkService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        public WikiLinkService(ILogger logger, ICoreConfiguration configuration, ISession databaseSession)
        {
            this.interwikiCache = new Dictionary<string, InterwikiPrefix>();
            this.lastLinkCache = new Dictionary<string, IEnumerable<string>>();
            this.logger = logger;
            this.databaseSession = databaseSession;

            this.defaultInterwikiPrefix =
                this.databaseSession.QueryOver<InterwikiPrefix>()
                    .Where(x => x.Prefix == configuration.DefaultInterwikiPrefix)
                    .SingleOrDefault();

            if (this.defaultInterwikiPrefix == null)
            {
                const string ErrorMessage = "Default InterWiki prefix is not found!";

                this.logger.Error(ErrorMessage);
                throw new ConfigurationException(ErrorMessage);
            }

            this.InterwikiCache.Add(this.defaultInterwikiPrefix.Prefix, this.defaultInterwikiPrefix);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="WikiLinkService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="databaseSession">
        /// The database session.
        /// </param>
        /// <param name="defaultPrefix">
        /// The default prefix.
        /// </param>
        public WikiLinkService(ILogger logger, ISession databaseSession, InterwikiPrefix defaultPrefix)
        {
            this.interwikiCache = new Dictionary<string, InterwikiPrefix>();
            this.lastLinkCache = new Dictionary<string, IEnumerable<string>>();
            this.logger = logger;
            this.databaseSession = databaseSession;

            this.defaultInterwikiPrefix = defaultPrefix;

            if (this.defaultInterwikiPrefix == null)
            {
                const string ErrorMessage = "Default InterWiki prefix is not found!";

                this.logger.Error(ErrorMessage);
                throw new ConfigurationException(ErrorMessage);
            }

            this.InterwikiCache.Add(this.defaultInterwikiPrefix.Prefix, this.defaultInterwikiPrefix);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the interwiki cache.
        /// </summary>
        public IDictionary<string, InterwikiPrefix> InterwikiCache
        {
            get
            {
                return this.interwikiCache;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The sanitise page title.
        /// </summary>
        /// <param name="pageTitle">
        /// The page title.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string SanitisePageTitle(string pageTitle)
        {
            pageTitle = pageTitle.Replace(' ', '_');
            var sanitisePageTitle = HttpUtility.UrlEncode(pageTitle);

            // ok, so the urlencode goes too far, let's take this back a notch.
            // we're only going to follow what MW does itself in wfUrlencode()
            sanitisePageTitle =
                sanitisePageTitle.Replace("%3b", ";")
                    .Replace("%40", "@")
                    .Replace("%24", "$")
                    .Replace("%21", "!")
                    .Replace("%2a", "*")
                    .Replace("%28", "(")
                    .Replace("%29", ")")
                    .Replace("%2c", ",")
                    .Replace("%2f", "/")
                    .Replace("%3a", ":");

            // .NET's urlencode doesn't go as far as PHP's, so let's adapt for that too
            sanitisePageTitle = sanitisePageTitle.Replace("'", "%27");

            return sanitisePageTitle;
        }

        /// <summary>
        /// The get last for channel.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        public IEnumerable<string> GetLastForChannel(string channel)
        {
            IEnumerable<string> data;
            var success = this.lastLinkCache.TryGetValue(channel, out data);
            return success ? data : new List<string>();
        }

        /// <summary>
        /// The get link.
        /// </summary>
        /// <param name="wikilink">
        /// The wikilink.
        /// </param>
        /// <returns>
        /// The <see cref="Uri"/>.
        /// </returns>
        public Uri GetLink(string wikilink)
        {
            var strings = wikilink.Split(new[] { ":" }, 2, StringSplitOptions.RemoveEmptyEntries);

            string sanitisedPage;

            if (strings.Count() > 1)
            {
                string prefix = strings[0];

                InterwikiPrefix interwikiPrefix;
                if (!this.InterwikiCache.TryGetValue(prefix, out interwikiPrefix))
                {
                    interwikiPrefix =
                        this.databaseSession.QueryOver<InterwikiPrefix>()
                            .Where(x => x.Prefix == prefix)
                            .SingleOrDefault();

                    this.InterwikiCache.Add(prefix, interwikiPrefix);
                }

                if (interwikiPrefix == null)
                {
                    interwikiPrefix = this.defaultInterwikiPrefix;
                    strings[1] = wikilink;
                }

                sanitisedPage = SanitisePageTitle(strings[1]);

                return new Uri(interwikiPrefix.GetUrl().Replace("$1", sanitisedPage));
            }

            sanitisedPage = SanitisePageTitle(wikilink);

            return new Uri(this.defaultInterwikiPrefix.GetUrl().Replace("$1", sanitisedPage));
        }

        /// <summary>
        /// Parses text and returns the page titles in any found wikilinks.
        /// </summary>
        /// <param name="input">
        /// The input text
        /// </param>
        /// <returns>
        /// A collection of page titles
        /// </returns>
        public IEnumerable<string> ParseForLinks(string input)
        {
            var linkRegex = new Regex(@"\[\[([^\[\]\|]*)(?:\]\]|\|)|{{([^{}\|]*)(?:}}|\|)");
            Match m = linkRegex.Match(input);
            while (m.Length > 0)
            {
                if (m.Groups[1].Length > 0)
                {
                    yield return m.Groups[1].Value;
                }

                if (m.Groups[2].Length > 0)
                {
                    yield return "Template:" + m.Groups[2].Value;
                }

                m = m.NextMatch();
            }
        }

        /// <summary>
        /// The parse incoming message.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public void ParseIncomingMessage(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Command == "PRIVMSG" || e.Message.Command == "NOTICE")
            {
                var parameters = e.Message.Parameters.ToList();

                string message = parameters[1];
                string channel = parameters[0];

                var newLinks = this.ParseForLinks(message).ToList();

                if (newLinks.Count == 0)
                {
                    this.logger.Debug("Found no links in incoming message.");
                    return;
                }

                this.logger.Info("Found links in incoming message, updating cache.");

                if (this.lastLinkCache.ContainsKey(channel))
                {
                    this.lastLinkCache.Remove(channel);
                }

                this.lastLinkCache.Add(channel, newLinks);

                // TODO: Autolinker when channel encapsulation is done.
            }
        }

        #endregion
    }
}