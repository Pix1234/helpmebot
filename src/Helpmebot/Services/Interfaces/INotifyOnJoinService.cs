// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INotifyOnJoinService.cs" company="Helpmebot Development Team">
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
//   The NotifyOnJoinService interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services.Interfaces
{
    using Helpmebot.IRC.Events;
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The NotifyOnJoinService interface.
    /// </summary>
    public interface INotifyOnJoinService
    {
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
        void OnJoinReceivedEvent(object sender, JoinEventArgs e);

        #endregion

        /// <summary>
        /// The add notification.
        /// </summary>
        /// <param name="triggerNickname">
        /// The trigger nickname.
        /// </param>
        /// <param name="toNotify">
        /// The to notify.
        /// </param>
        void AddNotification(string triggerNickname, IUser toNotify);
    }
}