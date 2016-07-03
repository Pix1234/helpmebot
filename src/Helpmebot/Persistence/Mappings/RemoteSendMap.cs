namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;

    using Helpmebot.Model;

    public class RemoteSendMap : ClassMap<RemoteSend>
    {
        public RemoteSendMap()
        {
            this.Id(x => x.Id, "id").GeneratedBy.GuidComb();
            this.References(x => x.OriginChannel, "originchannel");
            this.References(x => x.DestinationChannel, "destinationchannel");
        }
    }
}