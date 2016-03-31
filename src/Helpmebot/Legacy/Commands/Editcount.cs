﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Editcount.cs" company="Helpmebot Development Team">
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
namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;
    using System.Web;
    using System.Xml.XPath;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using NHibernate;

    using HttpRequest = Helpmebot.HttpRequest;

    /// <summary>
    ///     Returns the edit count of a Wikipedian
    /// </summary>
    [CommandInvocation("editcount")]
    [CommandFlag(Helpmebot.Model.Flag.Standard)]
    public class Editcount : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Editcount"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Editcount(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the edit count.
        /// </summary>
        /// <param name="username">
        /// The username to retrieve the edit count for.
        /// </param>
        /// <param name="channel">
        /// The channel the command was issued in. (Gets the correct base wiki)
        /// </param>
        /// <returns>
        /// The edit count
        /// </returns>
        [Obsolete("Please rewrite and fix ASAP")]
        public static int GetEditCount(string username, string channel)
        {
            if (username == string.Empty)
            {
                throw new ArgumentNullException();
            }

            // FIXME: servicelocator database
            var db = ServiceLocator.Current.GetInstance<ISession>();
            Channel channelObject = db.QueryOver<Channel>().Where(x => x.Name == channel).SingleOrDefault();
            if (channelObject == null)
            {
                throw new ArgumentOutOfRangeException("channel", "Unknown channel");
            }
            
            MediaWikiSite mediaWikiSite = channelObject.BaseWiki;

            username = HttpUtility.UrlEncode(username);

            // TODO: Linq-to-XML in MediaWikiSite extension method
            var uri = string.Format(
                "{0}?format=xml&action=query&list=users&usprop=editcount&format=xml&ususers={1}",
                mediaWikiSite.Api,
                username);

            using (var data = HttpRequest.Get(uri).ToStream())
            {
                var xpd = new XPathDocument(data);

                XPathNodeIterator xpni = xpd.CreateNavigator().Select("//user");

                if (xpni.MoveNext())
                {
                    string editcount = xpni.Current.GetAttribute("editcount", string.Empty);
                    if (editcount != string.Empty)
                    {
                        return int.Parse(editcount);
                    }

                    if (xpni.Current.GetAttribute("missing", string.Empty) == string.Empty)
                    {
                        // TODO: uint? rather than -1
                        return -1;
                    }
                }
            }

            throw new ArgumentException();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string userName;
            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                userName = string.Join(" ", this.Arguments);
            }
            else
            {
                userName = this.Source.Nickname;
            }

            int editCount = GetEditCount(userName, this.Channel);
            var messageService = this.CommandServiceHelper.MessageService;
            if (editCount == -1)
            {
                string[] messageParams = { userName };
                string message = messageService.RetrieveMessage("noSuchUser", this.Channel, messageParams);
                return new CommandResponseHandler(message);
            }
            else
            {
                var xtoolsUrl = string.Format("https://tools.wmflabs.org/xtools-ec/index.php?user={0}&project=en.wikipedia.org", userName);
                var xtoolsShortUrl = this.CommandServiceHelper.UrlShorteningService.Shorten(xtoolsUrl);

                string[] messageParameters = { editCount.ToString(CultureInfo.InvariantCulture), userName, xtoolsShortUrl };

                string message = messageService.RetrieveMessage("editCount", this.Channel, messageParameters);

                return new CommandResponseHandler(message);
            }
        }

        #endregion
    }
}