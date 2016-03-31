// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotifyOnJoinService.cs" company="Helpmebot Development Team">
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
//   The notify on join service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;

    using Castle.Core.Logging;

    using Helpmebot.IRC.Events;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;
    
    /// <summary>
    /// The notify on join service.
    /// </summary>
    public class NotifyOnJoinService : INotifyOnJoinService
    {
        #region Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// The requested notifications.
        /// </summary>
        private readonly IDictionary<string, List<IUser>> requestedNotifications;

        /// <summary>
        /// The lock object.
        /// </summary>
        private readonly object lockObject = new object();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="NotifyOnJoinService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public NotifyOnJoinService(ILogger logger, IMessageService messageService)
        {
            this.logger = logger;
            this.messageService = messageService;
            this.requestedNotifications = new Dictionary<string, List<IUser>>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on join received event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public void OnJoinReceivedEvent(object sender, JoinEventArgs e)
        {
            try
            {
                this.logger.Info("Searching notification list for requested pings");

                string search = e.User.Nickname.ToLowerInvariant();

                List<IUser> toNotify;
                lock (this.lockObject)
                {
                    if (this.requestedNotifications.TryGetValue(search, out toNotify))
                    {
                        this.requestedNotifications.Remove(search);
                    }
                }

                if (toNotify == null)
                {
                    this.logger.Info("Nothing found.");
                    return;
                }

                var message = this.messageService.RetrieveMessage(
                    "notifyJoin",
                    e.Channel,
                    new[] { e.User.Nickname, e.Channel });

                this.logger.InfoFormat("Found {0} ping(s), which I'll send now.", toNotify.Count);

                foreach (var user in toNotify)
                {
                    e.Client.SendMessage(user.Nickname, message);
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("Exception encountered in NotifyOnJoinEvent", exception);
            }
        }

        /// <summary>
        /// The add notification.
        /// </summary>
        /// <param name="triggerNickname">
        /// The trigger nickname.
        /// </param>
        /// <param name="toNotify">
        /// The to notify.
        /// </param>
        public void AddNotification(string triggerNickname, IUser toNotify)
        {
            string trigger = triggerNickname.ToLowerInvariant();

            lock (this.lockObject)
            {
                if (!this.requestedNotifications.ContainsKey(trigger))
                {
                    this.requestedNotifications.Add(trigger, new List<IUser>());
                }

                this.requestedNotifications[trigger].Add(toNotify);
            }

            this.logger.Info("Added notification");
        }

        #endregion
    }
}