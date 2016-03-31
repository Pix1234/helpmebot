// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainInstaller.cs" company="Helpmebot Development Team">
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
//   The main installer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Startup.Installers
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Castle.Core.Logging;
    using Castle.Facilities.EventWiring;
    using Castle.Facilities.Logging;
    using Castle.Facilities.Startable;
    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.Configuration.XmlSections.Interfaces;
    using Helpmebot.IRC;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Services;
    using Helpmebot.Startup.Facilities;
    using Helpmebot.Startup.Resolvers;
    using Helpmebot.TypedFactories;

    using helpmebot6.Commands;

    /// <summary>
    /// The main installer.
    /// </summary>
    public class MainInstaller : IWindsorInstaller
    {
        #region Fields

        /// <summary>
        /// The copyright.
        /// </summary>
        private readonly string copyright;

        /// <summary>
        /// The version.
        /// </summary>
        private readonly string version;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="MainInstaller"/> class. 
        /// </summary>
        public MainInstaller()
        {
            var helpmebotRuntime = Assembly.GetExecutingAssembly();

            this.version = helpmebotRuntime.GetName().Version.ToString();

            var copyAttributes = helpmebotRuntime.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (copyAttributes.Any())
            {
                var attribute = copyAttributes[0] as AssemblyCopyrightAttribute;
                if (attribute != null)
                {
                    this.copyright = attribute.Copyright;
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer"/>.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="store">
        /// The configuration store.
        /// </param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<LoggingFacility>(f => f.UseLog4Net().WithConfig("logger.config"))
                .AddFacility<EventWiringFacility>()
                .AddFacility<TypedFactoryFacility>();

            var installerLogger = container.Resolve<ILogger>();
            installerLogger.InfoFormat("Helpmebot v{0} {1}", this.version, this.copyright);

            // Configuration
            container.Kernel.Resolver.AddSubResolver(new AppSettingsDependencyResolver());

            var configurationHelper = new ConfigurationHelper();

            // Conventions
            var commands =
                Classes.FromThisAssembly()
                    .BasedOn<ICommand>()
                    .WithServiceSelf()
                    .WithServices(typeof(ICommand))
                    .LifestyleTransient();
            var legacyCommands =
                Classes.FromThisAssembly()
                    .BasedOn<GenericCommand>()
                    .WithServiceSelf()
                    .WithServices(typeof(ICommand))
                    .LifestyleTransient();

            container.Register(
                Component.For<IConfigurationHelper>().Instance(configurationHelper), 
                Component.For<ICoreConfiguration>().Instance(configurationHelper.CoreConfiguration), 
                Component.For<IPrivateConfiguration>().Instance(configurationHelper.PrivateConfiguration), 
                Component.For<IIrcConfiguration>().Instance(configurationHelper.IrcConfiguration), 
                Component.For<ILegacyDatabase>().ImplementedBy<LegacyDatabase>(), 
                Classes.FromThisAssembly().InNamespace("Helpmebot.Repositories").WithService.AllInterfaces(), 
                Classes.FromThisAssembly().InNamespace("Helpmebot.Services").WithService.AllInterfaces(), 
                Component.For<ICommandServiceHelper>().ImplementedBy<CommandServiceHelper>(), 
                Component.For<ICommandHandler>().ImplementedBy<CommandHandler>(), 
                Component.For<ICommandTypedFactory>().AsFactory(), 
                Component.For<IKeywordCommandFactory>().AsFactory(), 
                commands, 
                legacyCommands);

            Type networkClientType = configurationHelper.IrcConfiguration.Ssl
                                         ? typeof(SslNetworkClient)
                                         : typeof(NetworkClient);

            var networkClient =
                Component.For<INetworkClient>()
                    .ImplementedBy(networkClientType)
                    .DependsOn(
                        Dependency.OnValue("hostname", configurationHelper.IrcConfiguration.Hostname), 
                        Dependency.OnValue("port", configurationHelper.IrcConfiguration.Port));

            var ircClient =
                Component.For<IIrcClient>()
                    .ImplementedBy<IrcClient>()
                    .DependsOn(Dependency.OnValue("password", configurationHelper.PrivateConfiguration.IrcPassword))
                    .PublishEvent(
                        "ReceivedMessage", // Forced to use the string method with VS2015, or it becomes a runtime error
                        x =>
                        x.To<WikiLinkService>(l => l.ParseIncomingMessage(null, null))
                            .To<CommandHandler>(l => l.OnMessageReceived(null, null)))
                    .PublishEvent(
                        "JoinReceivedEvent", // Forced to use the string method with VS2015, or it becomes a runtime error
                        x =>
                        x.To<JoinMessageService>(l => l.WelcomeNewbieOnJoinEvent(null, null))
                            .To<NotifyOnJoinService>(l => l.OnJoinReceivedEvent(null, null)))
                    .PublishEvent(
                        "InviteReceivedEvent", // Forced to use the string method with VS2015, or it becomes a runtime error
                        x => x.To<InviteHandlerService>(l => l.OnInviteReceivedEvent(null, null)));

            container.Register(
                networkClient, 
                ircClient, 
                Classes.FromThisAssembly().InNamespace("Helpmebot.Background").WithService.AllInterfaces(), 
                Component.For<Application>());

            // This must come after the configuration has been registered.
            container.AddFacility<StartableFacility>(f => f.DeferredStart()).AddFacility<PersistenceFacility>();
        }

        #endregion
    }
}