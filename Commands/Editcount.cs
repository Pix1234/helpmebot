﻿#region Usings

using System;
using System.Reflection;
using System.Xml;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the edit count of a wikipedian
    /// </summary>
    internal class Editcount : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (args.Length > 0)
            {
                string userName = string.Join(" ", args);
                int editCount = getEditCount(userName, channel);
                if (editCount == -1)
                {
                    string[] messageParams = {userName};
                    string message = Configuration.singleton().getMessage("noSuchUser", messageParams);
                    return new CommandResponseHandler(message);
                }
                else
                {
                    string[] messageParameters = {editCount.ToString(), userName};

                    string message = Configuration.singleton().getMessage("editCount", messageParameters);

                    return new CommandResponseHandler(message);
                }
            }
            string[] messageParameters2 = {"count", "1", args.Length.ToString()};
            Helpmebot6.irc.ircNotice(source.nickname,
                                     Configuration.singleton().getMessage("notEnoughParameters", messageParameters2));
            return null;
        }

        public int getEditCount(string username, string channel)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (username == string.Empty)
            {
                throw new ArgumentNullException();
            }

            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            XmlTextReader creader =
                new XmlTextReader(
                    HttpRequest.get(api + "?format=xml&action=query&list=users&usprop=editcount&format=xml&ususers=" +
                                    username));
            do
            {
                creader.Read();
            } while (creader.Name != "user");
            string editCount = creader.GetAttribute("editcount");
            if (editCount != null)
            {
                return int.Parse(editCount);
            }
            if (creader.GetAttribute("missing") == "")
            {
                return -1;
            }
            throw new ArgumentException();
        }
    }
}