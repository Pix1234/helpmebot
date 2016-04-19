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
//   The application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IdleBot
{
    using System;
    using System.Threading;

    using Helpmebot.IRC.Interfaces;

    /// <summary>
    /// The application.
    /// </summary>
    public class Application : IApplication
    {
        /// <summary>
        /// The client.
        /// </summary>
        private readonly IIrcClient client;

        /// <summary>
        /// The exit flag.
        /// </summary>
        private readonly ManualResetEvent exitFlag;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        public Application(IIrcClient client)
        {
            this.client = client;
            this.exitFlag = new ManualResetEvent(false);
        }

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
        /// The run.
        /// </summary>
        public void Run()
        {
            this.client.JoinChannel("##stwalkerster");
            this.client.JoinChannel("##helpmebot");
            this.client.JoinChannel("#wikipedia-en-help");
            this.client.JoinChannel("#wikipedia-en-helpers");

            this.exitFlag.WaitOne();
        }
    }
}
