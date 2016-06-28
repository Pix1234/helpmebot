namespace Helpmebot.Commands.CategoryWatcher
{
    using System.Collections.Generic;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Model.Interfaces;

    using NHibernate;

    [CommandFlag(Model.Flag.Standard)]
    [CommandInvocation("fetchall")]
    public class CategoryWatcherFetchAllCommand : CommandBase
    {
        private readonly ICategoryWatcherBackgroundService categoryWatcherBackgroundService;

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
        /// <param name="categoryWatcherBackgroundService">
        /// The category watcher service
        /// </param>
        public CategoryWatcherFetchAllCommand(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            ILogger logger, 
            ISession databaseSession, 
            ICommandServiceHelper commandServiceHelper, 
            ICategoryWatcherBackgroundService categoryWatcherBackgroundService)
            : base(commandSource, user, arguments, logger, databaseSession, commandServiceHelper)
        {
            this.categoryWatcherBackgroundService = categoryWatcherBackgroundService;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            var watchersForChannel = this.categoryWatcherBackgroundService.GetWatchersForChannel(this.CommandChannel);

            foreach (var watcher in watchersForChannel.Values)
            {
                this.categoryWatcherBackgroundService.TriggerCategoryWatcherUpdate(watcher.Keyword, this.CommandChannel);
            }

            yield break;
        }
    }
}