// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Link.cs" company="Helpmebot Development Team">
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
//   Triggers the link parser
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Model.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Triggers the link parser
    /// </summary>
    [CommandInvocation("link")]
    [CommandFlag(Helpmebot.Model.Flag.Standard)]
    public class Link : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Link"/> class.
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
        public Link(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The result</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            // FIXME: servicelocator Wikilinkservice
            var linkService = ServiceLocator.Current.GetInstance<IWikiLinkService>();

            string[] args = this.Arguments;

            if (args.SmartLength() > 0)
            {
                IEnumerable<string> links = linkService.ParseForLinks(string.Join(" ", args)).ToList();

                if (links.Any())
                {
                    links = linkService.ParseForLinks("[[" + string.Join(" ", args) + "]]").ToList();
                }

                string message = links.Aggregate(
                    string.Empty,
                    (current, link) => current + " " + linkService.GetLink(link));

                return new CommandResponseHandler(message);
            }

            var lastForChannel = linkService.GetLastForChannel(this.Channel);

            string cached = lastForChannel
                .Aggregate(string.Empty, (current, link) => current + " " + linkService.GetLink(link));

            return new CommandResponseHandler(cached);
        }
    }
}