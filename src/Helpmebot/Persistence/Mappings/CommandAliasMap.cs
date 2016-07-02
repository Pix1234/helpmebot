namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;

    using Helpmebot.Model;

    public class CommandAliasMap : ClassMap<CommandAlias>
    {
        public CommandAliasMap()
        {
            this.Id(x => x.Id, "id").GeneratedBy.GuidComb();
            this.References(x => x.Channel, "channel");
            this.Map(x => x.Invocation, "invocation");
            this.Map(x => x.Target, "target");
        }
    }
}