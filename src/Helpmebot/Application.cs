// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Application.cs" company="Helpmebot Development Team">
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
//   The helpmebot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot
{
    using System;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;

    using NHibernate;

    /// <summary>
    /// The helpmebot.
    /// </summary>
    public class Application : IDisposable
    {
        #region Fields

        /// <summary>
        /// The database session.
        /// </summary>
        private readonly ISession databaseSession;

        /// <summary>
        /// The exit flag.
        /// </summary>
        private readonly ManualResetEvent exitFlag;

        /// <summary>
        /// The IRC client.
        /// </summary>
        private readonly IIrcClient ircClient;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The startup time.
        /// </summary>
        private readonly DateTime startupTime;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="ircClient">
        /// The IRC Client.
        /// </param>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        public Application(ILogger logger, IIrcClient ircClient, ISession databaseSession)
        {
            this.logger = logger;
            this.ircClient = ircClient;
            this.databaseSession = databaseSession;

            this.startupTime = DateTime.Now;

            this.exitFlag = new ManualResetEvent(false);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the exit flag.
        /// </summary>
        public ManualResetEvent ExitFlag
        {
            get
            {
                return this.exitFlag;
            }
        }

        /// <summary>
        /// Gets the startup time.
        /// </summary>
        public DateTime StartupTime
        {
            get
            {
                return this.startupTime;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The run.
        /// </summary>
        public void Run()
        {
            this.JoinChannels();

            this.exitFlag.WaitOne();

            this.logger.InfoFormat("Shutting down");
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable)this.exitFlag).Dispose();
            }
        }

        /// <summary>
        /// The join channels.
        /// </summary>
        private void JoinChannels()
        {
            var channels = this.databaseSession.QueryOver<Channel>().Where(x => x.Enabled).List();

            foreach (var channel in channels)
            {
                this.ircClient.JoinChannel(channel.Name);
            }
        }

        #endregion
    }
}