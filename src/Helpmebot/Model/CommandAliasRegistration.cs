namespace Helpmebot.Model
{
    internal class CommandAliasRegistration
    {
        /// <summary>Initializes a new instance of the <see cref="T:CommandAliasRegistration" /> class.</summary>
        public CommandAliasRegistration(string channel, string target)
        {
            this.Channel = channel;
            this.Target = target;
        }

        public string Channel { get; private set; }

        public string Target { get; private set; }
    }
}