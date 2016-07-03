namespace Helpmebot.Repositories
{
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using NHibernate;
    using NHibernate.Criterion;

    public class RemoteSendRepository : IRemoteSendRepository
    {
        private readonly IChannelRepository channelRepository;

        private readonly ISession databaseSession;

        public RemoteSendRepository(ISession databaseSession, IChannelRepository channelRepository)
        {
            this.databaseSession = databaseSession;
            this.channelRepository = channelRepository;
        }

        public bool CanRemoteSend(string from, string to)
        {
            var fromChannel = this.channelRepository.GetByName(from, this.databaseSession);
            var toChannel = this.channelRepository.GetByName(to, this.databaseSession);

            return
                this.databaseSession.CreateCriteria<RemoteSend>()
                    .Add(Restrictions.Eq("DestinationChannel", toChannel))
                    .Add(Restrictions.Eq("OriginChannel", fromChannel))
                    .List<RemoteSend>()
                    .Count > 0;
        }
    }
}