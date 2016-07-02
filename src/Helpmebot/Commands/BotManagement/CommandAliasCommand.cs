namespace Helpmebot.Commands.BotManagement
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
    using Helpmebot.Services.Interfaces;

    using NHibernate;
    using NHibernate.Criterion;

    [CommandInvocation("commandalias")]
    [CommandFlag(Model.Flag.Configuration)]
    public class CommandAliasCommand : CommandBase
    {
        private readonly ICommandParser commandParser;

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
        public CommandAliasCommand(
            string commandSource,
            IUser user,
            IEnumerable<string> arguments,
            ILogger logger,
            ISession databaseSession,
            ICommandServiceHelper commandServiceHelper,
            ICommandParser commandParser)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
            this.commandParser = commandParser;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            if (!this.Arguments.Any())
            {
                throw new ArgumentCountException(1, this.Arguments.Count());
            }

            bool global = false;
            var argList = this.Arguments.Skip(1).ToList();
            if (argList.Contains("--global"))
            {
                argList.Remove("--global");
                global = true;
            }

            switch (this.Arguments.First())
            {
                case "list":
                    return this.List(global);
                case "add":
                    if (this.Arguments.Count() < 3)
                    {
                        throw new ArgumentCountException(3, this.Arguments.Count(), "add");
                    }

                    return this.Add(global, argList);
                case "remove":
                case "delete":
                case "del":
                    if (this.Arguments.Count() < 2)
                    {
                        throw new ArgumentCountException(2, this.Arguments.Count(), "delete");
                    }

                    return this.Delete(global, argList);
                default:
                    throw new CommandInvocationException();
            }
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
                    new List<string> { "add [--global] <alias> <target>", "add [--global] <alias> null" },
                    "Adds a new alias to this channel (or globally). Using null will disable the command"));

            dict.Add(
                "delete",
                new HelpMessage(
                    this.CommandName,
                    "delete [--global] <alias>",
                    "Deletes an alias from this channel (or globally)"));

            dict.Add("list", new HelpMessage(this.CommandName, "list [--global]", "Lists the aliases configured"));

            return dict;
        }

        private IEnumerable<CommandResponse> Add(bool global, List<string> argumentsList)
        {
            var alias = argumentsList[0];
            var target = argumentsList[1];

            var list =
                this.DatabaseSession.CreateCriteria<CommandAlias>()
                    .Add(Restrictions.Eq("Channel", global ? null : this.CommandChannel))
                    .Add(Restrictions.Eq("Alias", alias))
                    .List<CommandAlias>()
                    .ToList();

            if (list.Count != 0)
            {
                throw new CommandErrorException("Cannot define duplicate alias in this channel");
            }

            // "null" is supported as a means to disable a command
            var realTarget = target == "null" ? null : target;

            var obj = new CommandAlias
                          {
                              Channel = global ? null : this.CommandChannel,
                              Invocation = alias,
                              Target = realTarget
                          };

            // TODO: register to command parser
            this.DatabaseSession.Save(obj);

            yield return
                new CommandResponse
                    {
                        Message =
                            string.Format(
                                "Created new {0}alias {1} for {2}",
                                global ? "global " : string.Empty,
                                alias,
                                target)
                    };
        }

        private IEnumerable<CommandResponse> Delete(bool global, List<string> argumentsList)
        {
            var alias = argumentsList[0];

            var list =
                this.DatabaseSession.CreateCriteria<CommandAlias>()
                    .Add(Restrictions.Eq("Channel", global ? null : this.CommandChannel))
                    .Add(Restrictions.Eq("Alias", alias))
                    .List<CommandAlias>()
                    .ToList();

            if (list.Count != 1)
            {
                throw new CommandErrorException("Ambiguous alias definition");
            }

            var commandAlias = list.First();

            this.DatabaseSession.Delete(commandAlias);

            // TODO: unregister from command parser
            yield return
                new CommandResponse
                    {
                        Message =
                            string.Format(
                                "Deleted alias {0}{1}",
                                commandAlias.Invocation,
                                global ? " globally" : string.Empty)
                    };
        }

        private IEnumerable<CommandResponse> List(bool global)
        {
            var list =
                this.DatabaseSession.CreateCriteria<CommandAlias>()
                    .Add(Restrictions.Eq("Channel", global ? null : this.CommandChannel))
                    .List<CommandAlias>()
                    .ToList();

            return
                list.Select(
                    x =>
                    new CommandResponse
                        {
                            Destination = CommandResponseDestination.PrivateMessage,
                            Message = string.Format("{0} => {1}", x.Invocation, x.Target ?? string.Empty)
                        });
        }
    }
}