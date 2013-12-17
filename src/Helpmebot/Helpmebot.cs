﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helpmebot.cs" company="Helpmebot Development Team">
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
//   Helpmebot main class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.Globalization;

    using Castle.Core.Logging;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    using Helpmebot.AI;
    using Helpmebot.IRC.Events;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.IRC;
    using Helpmebot.Monitoring;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Startup;
    using Helpmebot.Threading;

    using helpmebot6.Commands;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Helpmebot main class
    /// </summary>
    public class Helpmebot6
    {
        /// <summary>
        /// The start-up time.
        /// </summary>
        public static readonly DateTime StartupTime = DateTime.Now;

        /// <summary>
        /// The container.
        /// </summary>
        private static IWindsorContainer container;

        /// <summary>
        /// The IRC.
        /// </summary>
        private static IrcAccessLayer irc;

        /// <summary>
        /// The DB access layer.
        /// </summary>
        private static DAL dbal;

        /// <summary>
        /// The debug channel.
        /// </summary>
        private static string debugChannel;

        /// <summary>
        /// The IRC network.
        /// </summary>
        private static uint ircNetwork;

        /// <summary>
        /// The join message service.
        /// </summary>
        /// <para>
        /// This is the replacement for the newbiewelcomer
        /// </para>
        private static IJoinMessageService joinMessageService;

        /// <summary>
        /// Gets or sets the trigger.
        /// </summary>
        public static string Trigger { get; set; }

        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        /// The stop.
        /// </summary>
        public static void Stop()
        {
            ThreadList.instance().stop();
        }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            BootstrapContainer();

            ServiceLocator.Current.GetInstance<ILogger>().Info("Initialising Helpmebot...");

            InitialiseBot();
        }

        /// <summary>
        /// The bootstrap container.
        /// </summary>
        private static void BootstrapContainer()
        {
            container = new WindsorContainer();

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            container.Install(FromAssembly.This(new WindsorBootstrap()));
        }

        /// <summary>
        /// The initialise bot.
        /// </summary>
        private static void InitialiseBot()
        {
            dbal = DAL.singleton();

            if (!dbal.connect())
            {
                // can't connect to database, DIE
                return;
            }

            LegacyConfig.singleton();

            debugChannel = LegacyConfig.singleton()["channelDebug"];

            ircNetwork = uint.Parse(LegacyConfig.singleton()["ircNetwork"]);

            Trigger = LegacyConfig.singleton()["commandTrigger"];

            irc = new IrcAccessLayer(ircNetwork);

            // TODO: remove me!
            container.Register(Component.For<IrcAccessLayer>().Instance(irc));

            joinMessageService = container.Resolve<IJoinMessageService>();

            SetupEvents();

            if (!irc.Connect())
            {
                // if can't connect to irc, die
                return;
            }

            new MonitorService(62167, "Helpmebot v6 (Nagios Monitor service)");

            // ACC notification monitor
            AccNotifications.getInstance();
        }

        /// <summary>
        /// The setup events.
        /// </summary>
        private static void SetupEvents()
        {
            irc.ConnectionRegistrationSucceededEvent += JoinChannels;

            irc.JoinEvent += WelcomeNewbieOnJoinEvent;

            irc.JoinEvent += NotifyOnJoinEvent;

            irc.PrivateMessageEvent += ReceivedMessage;

            irc.InviteEvent += IrcInviteEvent;

            irc.ThreadFatalErrorEvent += IrcThreadFatalErrorEvent;
        }

        /// <summary>
        /// The IRC thread fatal error event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void IrcThreadFatalErrorEvent(object sender, EventArgs e)
        {
            Stop();
        }

        /// <summary>
        /// The IRC invite event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private static void IrcInviteEvent(User source, string nickname, string channel)
        {
            // FIXME: Remove service locator!
            new Join(source, nickname, new[] { channel }, ServiceLocator.Current.GetInstance<IMessageService>()).RunCommand();
        }

        /// <summary>
        /// The welcome newbie on join event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private static void WelcomeNewbieOnJoinEvent(User source, string channel)
        {
            joinMessageService.Welcome(source, channel);
        }

        /// <summary>
        /// The notify on join event.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        private static void NotifyOnJoinEvent(User source, string channel)
        {
            // FIXME: Remove service locator!
            new Notify(source, channel, new string[0], ServiceLocator.Current.GetInstance<IMessageService>()).NotifyJoin(source, channel);
        }

        /// <summary>
        /// The received message.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void ReceivedMessage(object sender, PrivateMessageEventArgs e)
        {
            string message = e.Message;

            var cmd = new CommandParser();
            try
            {
                bool overrideSilence = cmd.OverrideBotSilence;
                if (CommandParser.isRecognisedMessage(ref message, ref overrideSilence))
                {
                    cmd.OverrideBotSilence = overrideSilence;
                    string[] messageWords = message.Split(' ');
                    string command = messageWords[0];
                    string joinedargs = string.Join(" ", messageWords, 1, messageWords.Length - 1);
                    string[] commandArgs = joinedargs == string.Empty ? new string[0] : joinedargs.Split(' ');

                    cmd.handleCommand(e.Sender, e.Destination, command, commandArgs);
                }

                string intelligenceResponse = Intelligence.Singleton().Respond(message);
                if (LegacyConfig.singleton()["silence", e.Destination] == "false" && intelligenceResponse != string.Empty)
                {
                    string[] intelligenceParameters = { e.Sender.nickname };
                    var messageService = ServiceLocator.Current.GetInstance<IMessageService>(); // TODO: fix me
                    irc.IrcPrivmsg(e.Destination, messageService.RetrieveMessage(intelligenceResponse, e.Destination, intelligenceParameters));
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.Current.GetInstance<ILogger>().Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// The join channels.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void JoinChannels(object sender, EventArgs e)
        {
            irc.IrcJoin(debugChannel);

            var q = new DAL.Select("channel_name");
            q.setFrom("channel");
            q.addWhere(new DAL.WhereConds("channel_enabled", 1));
            q.addWhere(new DAL.WhereConds("channel_network", ircNetwork.ToString(CultureInfo.InvariantCulture)));
            foreach (object[] item in dbal.executeSelect(q))
            {
                irc.IrcJoin((string)item[0]);
            }
        }
    }
}
