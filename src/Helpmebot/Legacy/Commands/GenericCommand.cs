// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericCommand.cs" company="Helpmebot Development Team">
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
//   Generic bot command abstract class
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Model.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    using NHibernate;

    /// <summary>
    ///     Generic bot command abstract class
    /// </summary>
    public abstract class GenericCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="GenericCommand"/> class.
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
        /// The command Service Helper.
        /// </param>
        protected GenericCommand(
            IUser source, 
            string channel, 
            string[] args, 
            ICommandServiceHelper commandServiceHelper)
            : base(
                channel, 
                source, 
                args, 
                ServiceLocator.Current.GetInstance<ILogger>(), 
                ServiceLocator.Current.GetInstance<ISession>(), 
                commandServiceHelper)
        {
            this.Source = source;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        public new string[] Arguments
        {
            get
            {
                return base.Arguments.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        public string Channel
        {
            get
            {
                return this.CommandSource;
            }
        }

        /// <summary>
        ///     Gets or sets the source.
        /// </summary>
        public IUser Source { get; set; }

        #endregion

        #region Public Methods and Operators

        #endregion

        #region Methods

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            var commandResponseHandler = this.ExecuteCommand();

            return commandResponseHandler == null
                       ? new List<CommandResponse>()
                       : commandResponseHandler.GetResponses().OfType<CommandResponse>();
        }

        /// <summary>
        ///     The execute command.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected virtual CommandResponseHandler ExecuteCommand()
        {
            return new CommandResponseHandler("not implemented");
        }

        /// <summary>
        /// The on access denied.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> OnAccessDenied()
        {
            var commandResponseHandler = this.OnLegacyAccessDenied();

            return commandResponseHandler == null
                       ? new List<CommandResponse>()
                       : commandResponseHandler.GetResponses().OfType<CommandResponse>();
        }

        /// <summary>
        ///     Access denied to command, decide what to do
        /// </summary>
        /// <returns>A response to the command if access to the command was denied</returns>
        protected virtual CommandResponseHandler OnLegacyAccessDenied()
        {
            var response = new CommandResponseHandler();

            string message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                "AccessDenied", 
                this.Channel, 
                null);

            response.Respond(message, CommandResponseDestination.PrivateMessage);
            this.Logger.Info("Access denied to command.");

            return response;
        }

        #endregion
    }
}