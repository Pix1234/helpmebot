// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JoinCommand.cs" company="Helpmebot Development Team">
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
//   The join command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Commands.ChannelManagement
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.IRC.Messages;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    using NHibernate;

    /// <summary>
    /// The join command.
    /// </summary>
    [CommandFlag(Model.Flag.LegacySuperuser)]
    [CommandInvocation("join")]
    public class JoinCommand : CommandBase, IDisposable
    {
        /// <summary>
        /// The message timeout lock.
        /// </summary>
        private readonly object messageTimeoutLock = new object();

        /// <summary>
        /// The channel to join.
        /// </summary>
        /// <remarks>
        /// This is NOT a duplicate of the channel property, as that holds the channel
        /// the command was EXECUTED in, not the channel to JOIN.
        /// </remarks>
        private Channel channel;

        /// <summary>
        /// The channel name.
        /// </summary>
        private string channelName;

        /// <summary>
        /// The message timeout.
        /// </summary>
        private int messageTimeout = 30;

        /// <summary>
        /// Initialises a new instance of the <see cref="JoinCommand"/> class.
        /// </summary>
        /// <param name="commandSource">
        /// The command source.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        /// <param name="commandServiceHelper">
        /// The command Service Helper.
        /// </param>
        public JoinCommand(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            ILogger logger, 
            ISession databaseSession, 
            ICommandServiceHelper commandServiceHelper)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
        }
        
        /// <summary>
        /// Returns true if the command can be executed.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool CanExecute()
        {
            if (!base.CanExecute())
            {
                return false;
            }

            var args = this.Arguments.ToList();
            if (args.Count >= 1)
            {
                if (args[0] == "0")
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)this.CommandCompletedSemaphore).Dispose();
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            var arguments = this.Arguments.ToList();

            if (arguments.Count >= 1)
            {
                this.channelName = arguments[0];

                this.channel = this.CommandServiceHelper.ChannelRepository.GetByName(this.channelName, this.DatabaseSession);

                if (this.channel != null)
                {
                    this.channel.Enabled = true;
                }
                else
                {
                    this.channel = new Channel { Enabled = true, Name = this.channelName };
                }

                this.DatabaseSession.Save(this.channel);

                var ircClient = this.CommandServiceHelper.Client;
                ircClient.ReceivedMessage += this.OnIrcReceivedMessage;

                ircClient.JoinChannel(this.channelName);

                // don't kill the session yet!
                this.CommandCompletedSemaphore.WaitOne();

                return null;
            }

            string[] messageParameters = { "join", "1", arguments.Count.ToString(CultureInfo.InvariantCulture) };
            var message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                Messages.NotEnoughParameters, 
                this.CommandChannel, 
                messageParameters);

            return new CommandResponse { Message = message }.ToEnumerable();
        }

        /// <summary>
        /// Monitors communication after the Join command for error 
        /// messages relating to this command's execution. 
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnIrcReceivedMessage(object sender, MessageReceivedEventArgs e)
        {
            switch (e.Message.Command)
            {
                case Numerics.ForwardingToOtherChannel:
                    if (this.IntendedForUs(e.Message.Parameters, e.Client.Nickname))
                    {
                        this.OnChannelForward(e.Message.Parameters, e.Client);
                    }

                    break;
                case Numerics.ChannelFull:
                    if (this.IntendedForUs(e.Message.Parameters, e.Client.Nickname))
                    {
                        this.OnChannelFull(e.Message.Parameters, e.Client);
                    }

                    break;
                case Numerics.InviteOnlyChannel:
                    if (this.IntendedForUs(e.Message.Parameters, e.Client.Nickname))
                    {
                        this.OnInviteOnlyChannel(e.Message.Parameters, e.Client);
                    }

                    break;
                case Numerics.BannedFromChannel:
                    if (this.IntendedForUs(e.Message.Parameters, e.Client.Nickname))
                    {
                        this.OnBannedFromChannel(e.Message.Parameters, e.Client);
                    }

                    break;
                case Numerics.BadChannelKey:
                    if (this.IntendedForUs(e.Message.Parameters, e.Client.Nickname))
                    {
                        this.OnBadChannelKey(e.Message.Parameters, e.Client);
                    }

                    break;
                case Numerics.ChannelThrottleExceeded:
                    if (this.IntendedForUs(e.Message.Parameters, e.Client.Nickname))
                    {
                        this.OnChannelThrottleExceeded(e.Message.Parameters, e.Client);
                    }

                    break;
                case "JOIN":
                    if (this.IntendedForUs(e.Message.Parameters, e.Client.Nickname))
                    {
                        // success! unsubscribe silently.
                        lock (this.messageTimeoutLock)
                        {
                            this.messageTimeout = 0;
                        }
                    }

                    break;
            }

            lock (this.messageTimeoutLock)
            {
                if (this.messageTimeout == 0)
                {
                    // unsubscribe from the event
                    e.Client.ReceivedMessage -= this.OnIrcReceivedMessage;

                    // allow the main command to exit.
                    this.CommandCompletedSemaphore.Release();
                }
                else
                {
                    // not anything we're interested in. Decrement the TTL, and carry on.
                    this.messageTimeout--;
                }
            }
        }

        /// <summary>
        /// The intended for us.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IntendedForUs(IEnumerable<string> parameters, string nickname)
        {
            var list = parameters.ToList();

            if (list[0] != nickname)
            {
                return false;
            }

            if (list[1] != this.channelName)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The on channel throttle exceeded.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        private void OnChannelThrottleExceeded(IEnumerable<string> parameters, IIrcClient client)
        {
            // Mention the fullness to the user.
            var parametersList = parameters.ToList();
            client.SendMessage(this.User.Nickname, parametersList[2]);
            this.Logger.WarnFormat(parametersList[2]);

            // Finally, unsubscribe the handler.
            lock (this.messageTimeoutLock)
            {
                this.messageTimeout = 0;
            }
        }

        /// <summary>
        /// The on bad channel key.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        private void OnBadChannelKey(IEnumerable<string> parameters, IIrcClient client)
        {
            // Disable the channel in the configuration
            this.channel.Enabled = false;
            this.DatabaseSession.Save(this.channel);

            // Mention the fullness to the user.
            var parametersList = parameters.ToList();
            client.SendMessage(this.User.Nickname, parametersList[2]);
            this.Logger.WarnFormat(parametersList[2]);

            // Finally, unsubscribe the handler.
            lock (this.messageTimeoutLock)
            {
                this.messageTimeout = 0;
            }
        }

        /// <summary>
        /// The on banned from channel.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        private void OnBannedFromChannel(IEnumerable<string> parameters, IIrcClient client)
        {
            // Mention the banned.
            var parametersList = parameters.ToList();
            client.SendMessage(this.User.Nickname, parametersList[2]);
            this.Logger.WarnFormat(parametersList[2]);

            // Disable the channel in the configuration
            this.channel.Enabled = false;
            this.DatabaseSession.Save(this.channel);

            // Finally, unsubscribe the handler.
            lock (this.messageTimeoutLock)
            {
                this.messageTimeout = 0;
            }
        }

        /// <summary>
        /// The on invite only channel.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        private void OnInviteOnlyChannel(IEnumerable<string> parameters, IIrcClient client)
        {
            // Disable the channel in the configuration
            this.channel.Enabled = false;
            this.DatabaseSession.Save(this.channel);

            // Mention the invite only problem to the user.
            var parametersList = parameters.ToList();
            client.SendMessage(this.User.Nickname, parametersList[2]);
            this.Logger.WarnFormat(parametersList[2]);

            // Finally, unsubscribe the handler.
            lock (this.messageTimeoutLock)
            {
                this.messageTimeout = 0;
            }
        }

        /// <summary>
        /// The on channel full.
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        private void OnChannelFull(IEnumerable<string> parameters, IIrcClient client)
        {
            // Mention the fullness to the user.
            var parametersList = parameters.ToList();
            client.SendMessage(this.User.Nickname, parametersList[2]);
            this.Logger.WarnFormat(parametersList[2]);

            // Finally, unsubscribe the handler.
            lock (this.messageTimeoutLock)
            {
                this.messageTimeout = 0;
            }
        }

        /// <summary>
        /// Called in response to a forward message.
        /// <para>
        /// Eek. Forwards. This is awkward. Do we copy settings, or treat as new?
        /// </para>
        /// <para>
        /// Forwards are used in two ways AFAIK:
        ///    1) We're going to send a problem elsewhere.
        ///    2) We've moved.
        /// </para>
        /// <para>
        /// In the first, we really want to treat it as a new channel - old settings
        /// could be harmful. In the second, it'd be nice to keep the settings, but
        /// it shouldn't matter too much if they're lost. Therefore, forwarded channels 
        /// are treated as new channels.
        /// </para>
        /// </summary>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        private void OnChannelForward(IEnumerable<string> parameters, IIrcClient client)
        {
            var parametersList = parameters.ToList();

            // Firstly, disable the old channel
            this.channel.Enabled = false;
            this.DatabaseSession.Save(this.channel);

            // Secondly, set up the new channel.
            var newChannel = this.CommandServiceHelper.ChannelRepository.GetByName(parametersList[2], this.DatabaseSession);

            if (newChannel != null)
            {
                newChannel.Enabled = true;
            }
            else
            {
                newChannel = new Channel { Enabled = true, Name = parametersList[2] };
            }

            this.DatabaseSession.Save(newChannel);

            // Mention the forward to the person who asked us to join.
            client.SendMessage(this.User.Nickname, parametersList[2]);
            this.Logger.WarnFormat("Forwarding join from {0} to {1}", this.channelName, parametersList[2]);

            // Finally, unsubscribe the handler.
            lock (this.messageTimeoutLock)
            {
                this.messageTimeout = 0;
            }
        }
    }
}