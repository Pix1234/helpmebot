namespace Helpmebot.Model
{
    using Helpmebot.Persistence;

    public class RemoteSend : GuidEntityBase
    {
        public virtual Channel DestinationChannel { get; set; }

        public virtual Channel OriginChannel { get; set; }
    }
}