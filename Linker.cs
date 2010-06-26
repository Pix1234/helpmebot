﻿#region Usings

using System.Collections.Generic;
using System.Reflection;

#endregion

namespace helpmebot6
{
    public class Linker
    {
        private readonly Dictionary<string, string> _lastLink;

        private static Linker _singleton;

        protected Linker()
        {
            this._lastLink = new Dictionary<string, string>();
            Helpmebot6.irc.privmsgEvent += irc_PrivmsgEvent;
            Helpmebot6.irc.noticeEvent += irc_PrivmsgEvent;
        }

        private void irc_PrivmsgEvent(User source, string destination, string message)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            this.parseMessage(message, destination);
        }

        public static Linker instance()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return _singleton ?? ( _singleton = new Linker( ) );
        }

        public void parseMessage(string message, string channel)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string newLink = reallyParseMessage(message);
            if ( newLink == "" ) return;
            if (this._lastLink.ContainsKey(channel))
            {
                this._lastLink.Remove(channel);
            }
            this._lastLink.Add(channel, newLink);
            this.sendLink(channel, newLink);
        }

        public string reallyParseMessage(string message)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string newLink = "";

            if ((message.Contains("[[") && message.Contains("]]")))
            {
                // [[newLink]]

                int startIndex = message.IndexOf("[[");
                int endIndex = message.IndexOf("]]", startIndex);

                if (endIndex != -1)
                {
                    int nextStartIndex = message.IndexOf("[[", startIndex + 2, endIndex - startIndex);

                    while (nextStartIndex != -1)
                    {
                        startIndex = nextStartIndex;
                        nextStartIndex = message.IndexOf("[[", startIndex + 2, endIndex - startIndex);
                    }

                    newLink = message.Substring(startIndex + 2, endIndex - startIndex - 2);
                }
            }
            if ((message.Contains("{{") && message.Contains("}}")))
            {
                int startIndex = message.IndexOf("{{");
                int endIndex = message.IndexOf("}}", startIndex);
                if (endIndex != -1)
                {
                    int nextStartIndex = message.IndexOf("{{", startIndex + 2, endIndex - startIndex);

                    while (nextStartIndex != -1)
                    {
                        startIndex = nextStartIndex;
                        nextStartIndex = message.IndexOf("{{", startIndex + 2, endIndex - startIndex);
                    }

                    newLink = "Template:" + message.Substring(startIndex + 2, endIndex - startIndex - 2);
                }
            }
            newLink = newLink.Trim('[');
            return newLink;
        }

        public string getLink(string destination)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return this.getLink(destination, false);
        }

        public string getLink(string destination, bool useSecureServer)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            string link;
            bool success = this._lastLink.TryGetValue(destination, out link);
            if (success)
            {
                string iwprefix = link.Split(':')[0];

                DAL.Select q = new DAL.Select("iw_url");
                q.setFrom("interwikis");
                q.addWhere(new DAL.WhereConds("iw_prefix", iwprefix));
                string url = DAL.singleton().executeScalarSelect(q);

                if (url == string.Empty)
                {
                    url =
                        Configuration.singleton().retrieveLocalStringOption(
                            (useSecureServer ? "wikiSecureUrl" : "wikiUrl"), destination);
                    return url + antispace(link);
                }
                return url.Replace("$1", antispace(string.Join(":", link.Split(':'), 1, link.Split(':').Length - 1)));
            }
            return "";
        }

        private static string antispace(string source)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            int currloc = 0;
            string result = "";
            while (currloc < source.Length)
            {
                if (source.Substring(currloc, 1) == " ")
                {
                    result += "_";
                }
                else
                {
                    result += source.Substring(currloc, 1);
                }
                currloc += 1;
            }
            return result;
        }

        private void sendLink(string channel, string link)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (Configuration.singleton().retrieveLocalStringOption("autoLink", channel) == "true")
                Helpmebot6.irc.ircPrivmsg(channel, this.getLink(link, false));
        }
    }
}