namespace Helpmebot.Commands.AccessControl
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    using NHibernate;
    using NHibernate.Criterion;

    [CommandInvocation("remotesend")]

    // TODO: fix this once T193 is done
    [CommandFlag(Model.Flag.Owner)]
    public class RemoteSendCommand : CommandBase
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CommandBase"/> class.
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
        public RemoteSendCommand(
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
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            // OK, this *must* be executed from the destination channel
            var destination = this.CommandChannel;

            if (!this.Arguments.Any())
            {
                throw new ArgumentCountException(2, this.Arguments.Count());
            }

            var mode = this.Arguments.ElementAt(0).ToLowerInvariant();

            if (mode == "add")
            {
                if (this.Arguments.Count() < 2)
                {
                    throw new ArgumentCountException(2, this.Arguments.Count(), "add");
                }

                return this.AddRemoteSendOrigin(destination, this.Arguments.Skip(1).FirstOrDefault());
            }

            if (mode == "del" || mode == "delete" || mode == "remove")
            {
                if (this.Arguments.Count() < 2)
                {
                    throw new ArgumentCountException(2, this.Arguments.Count(), "delete");
                }

                return this.DeleteRemoteSendOrigin(destination, this.Arguments.Skip(1).FirstOrDefault());
            }

            throw new CommandInvocationException();
        }

        /// <summary>
        /// The help.
        /// </summary>
        /// <returns>
        /// The <see cref="IDictionary{String, HelpMessage}"/>.
        /// </returns>
        protected override IDictionary<string, HelpMessage> Help()
        {
            var dict = new Dictionary<string, HelpMessage>();

            dict.Add(
                "add",
                new HelpMessage(
                    this.CommandName,
                    "add <channel>",
                    "Allows a channel to send messages to this channel"));

            dict.Add(
                "delete",
                new HelpMessage(
                    this.CommandName,
                    "delete <channel>",
                    "Stops a channel sending messages to this channel"));

            return dict;
        }

        private IEnumerable<CommandResponse> AddRemoteSendOrigin(Channel destination, string originName)
        {
            var origin = this.GetChannel(originName);
            var remoteSends = this.GetExistingRemoteSends(destination, origin).ToList();

            if (remoteSends.Count > 0)
            {
                yield return new CommandResponse { Message = "Remote send configuration for this pair already exists" };
                yield break;
            }

            var remoteSend = new RemoteSend { DestinationChannel = destination, OriginChannel = origin };
            this.DatabaseSession.SaveOrUpdate(remoteSend);

            yield return new CommandResponse { Message = "Remote send configuration created" };
        }

        private IEnumerable<CommandResponse> DeleteRemoteSendOrigin(Channel destination, string originName)
        {
            var origin = this.GetChannel(originName);
            var remoteSends = this.GetExistingRemoteSends(destination, origin).ToList();

            if (remoteSends.Count == 0)
            {
                yield return new CommandResponse { Message = "No remote send configurations for this pair found" };
                yield break;
            }

            if (remoteSends.Count > 1)
            {
                yield return new CommandResponse { Message = "Ambiguous remote send configuration." };
                yield break;
            }

            this.DatabaseSession.Delete(remoteSends.First());

            yield return new CommandResponse { Message = "Remote send configuration deleted" };
        }

        private Channel GetChannel(string originName)
        {
            return
                this.DatabaseSession.CreateCriteria<Channel>()
                    .Add(Restrictions.Eq("Name", originName))
                    .Add(Restrictions.Eq("Enabled", true))
                    .List<Channel>()
                    .FirstOrDefault();
        }

        private IEnumerable<RemoteSend> GetExistingRemoteSends(Channel destination, Channel origin)
        {
            var remoteSends =
                this.DatabaseSession.CreateCriteria<RemoteSend>()
                    .Add(Restrictions.Eq("OriginChannel", origin))
                    .Add(Restrictions.Eq("DestinationChannel", destination))
                    .List<RemoteSend>()
                    .ToList();

            return remoteSends;
        }
    }
}