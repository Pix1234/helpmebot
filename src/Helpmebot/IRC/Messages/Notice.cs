namespace Helpmebot.IRC.Messages
{
    using System.Collections.Generic;

    internal class Notice : Message
    {
        public Notice(string destination, string message)
            : base("NOTICE", new List<string> { destination, message })
        {
        }
    }
}