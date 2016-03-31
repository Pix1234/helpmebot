// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Forget.cs" company="Helpmebot Development Team">
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
//   Forgets a keyword
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///   Forgets a keyword
    /// </summary>
    [CommandInvocation("forget")]
    [CommandFlag(Helpmebot.Model.Flag.LegacySuperuser)]
    public class Forget : GenericCommand
    {
        /// <summary>
        /// The keyword service.
        /// </summary>
        private readonly IKeywordService keywordService;

        /// <summary>
        /// Initialises a new instance of the <see cref="Forget"/> class.
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
        /// <param name="keywordService">
        /// The keyword Service.
        /// </param>
        public Forget(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper, IKeywordService keywordService)
            : base(source, channel, args, commandServiceHelper)
        {
            this.keywordService = keywordService;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var messageService = this.CommandServiceHelper.MessageService;
            if (this.Arguments.Length >= 1)
            {
                string forgottenMessage;
                try
                {
                    foreach (var argument in this.Arguments)
                    {
                        this.keywordService.Delete(argument);
                    }

                    forgottenMessage = messageService.RetrieveMessage("cmdForgetDone", this.Channel, null);
                }
                catch (Exception ex)
                {
                    this.Logger.Error("Error forgetting keyword", ex);
                    forgottenMessage = messageService.RetrieveMessage("cmdForgetError", this.Channel, null);
                }
                
                this.CommandServiceHelper.Client.SendNotice(this.Source.Nickname, forgottenMessage);
            }
            else
            {
                string[] messageParameters = { "forget", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture) };
                this.CommandServiceHelper.Client.SendNotice(
                    this.Source.Nickname,
                    messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            return null;
        }
    }
}
