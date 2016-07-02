namespace Helpmebot.Model
{
    using Helpmebot.Persistence;

    public class CommandAlias : GuidEntityBase
    {
        public virtual Channel Channel { get; set; }

        public virtual string Invocation { get; set; }

        public virtual string Target { get; set; }
    }
}