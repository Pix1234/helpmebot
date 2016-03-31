// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientApplication.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   The client application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Client
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;

    using NHibernate;

    /// <summary>
    /// The client application.
    /// </summary>
    public class ClientApplication : IApplication
    {
        #region Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The database session factory.
        /// </summary>
        private readonly ISessionFactory databaseSessionFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="ClientApplication"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="databaseSessionFactory">
        /// The database session Factory.
        /// </param>
        public ClientApplication(ILogger logger, ISessionFactory databaseSessionFactory)
        {
            this.logger = logger;
            this.databaseSessionFactory = databaseSessionFactory;
        }

        #endregion

        /// <summary>
        /// The run.
        /// </summary>
        public void Run()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("Helpmebot Client");
                Console.WriteLine();
                Console.WriteLine("Please choose an option:");
                Console.WriteLine("   0: Exit");
                Console.WriteLine("   1: Recreate permissions");
                Console.WriteLine("   2: Migrate sites and channels");
                Console.WriteLine();
                Console.Write("Choice [0]: ");
                var readLine = Console.ReadLine();

                if (string.IsNullOrEmpty(readLine) || readLine == "0")
                {
                    break;
                }

                if (readLine == "1")
                {
                    this.DoTransactionally(this.RecreatePermissions);
                }

                if (readLine == "2")
                {
                    this.DoTransactionally(this.MigrateSitesAndChannels);
                }

                Console.Clear();
            }
        }

        /// <summary>
        /// do something in a transaction.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        private void DoTransactionally(Action<ISession> action)
        {
            this.logger.InfoFormat("Opening database connection, and starting transaction");
            var database = this.databaseSessionFactory.OpenSession();
            var transaction = database.BeginTransaction(IsolationLevel.RepeatableRead);
            try
            {
                action(database);

                transaction.Commit();
                database.Close();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                this.logger.Error(e.Message, e);
                database.Close();
            }

            this.logger.Info("Done. Press a key to continue.");
            Console.ReadLine();
        }

        /// <summary>
        /// The migrate channels.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        private void MigrateSitesAndChannels(ISession database)
        {
            this.logger.Info("Clearing old sites and channels");

            var deleteChannels = database.CreateSQLQuery("DELETE FROM channelnew");
            deleteChannels.ExecuteUpdate();

            var deleteSites = database.CreateSQLQuery("DELETE FROM mediawikisite");
            deleteSites.ExecuteUpdate();

            this.logger.Info("Importing sites");

            var legacySites = database.CreateSQLQuery(@"
SELECT
    site_id oldid
  , CASE site_username WHEN '' THEN NULL ELSE site_username END username
  , CASE site_password WHEN '' THEN NULL ELSE site_password END password
  , site_api api
  , SUBSTRING_INDEX(SUBSTR(REPLACE(REPLACE(site_mainpage, 'https', 'http'), 'http://','') FROM 1 FOR 45), '/', 1) name
FROM site;");

            var sites = legacySites.List();

            var lookupCache = new Dictionary<int, MediaWikiSite>();

            foreach (var obj in sites)
            {
                var oldSite = (object[])obj;

                var newSite = new MediaWikiSite
                {
                    Username = (string)oldSite[1],
                    Password = (string)oldSite[2],
                    Api = (string)oldSite[3],
                    Name = (string)oldSite[4],
                };

                database.Save(newSite);

                lookupCache.Add((int)oldSite[0], newSite);
            }

            this.logger.Info("Importing channels");

            var legacyChannels = database.CreateSQLQuery(@"
SELECT
    channel_name        name
  , CASE channel_password
		WHEN '' THEN null
        ELSE channel_password
	END password
  , channel_enabled     enabled
  , CASE(   SELECT cc_value
            FROM   channelconfig cc
            INNER JOIN configuration cfg ON
                    cc_config = configuration_id
                AND configuration_name = 'silence'
            WHERE  cc.cc_channel = c.channel_id)
        WHEN 'false' THEN 0
        WHEN 'true'  THEN 1
        ELSE              0
        END             silence
  , COALESCE(
		(	SELECT cc.cc_value
			FROM   channelconfig cc
			INNER JOIN configuration cfg ON
					cc_config = configuration_id
				AND configuration_name = 'baseWiki'
			WHERE  cc.cc_channel = c.channel_id )
	  , (	SELECT cfg.configuration_value 
			FROM configuration cfg 
			WHERE cfg.configuration_name = 'baseWiki'))
                        basewiki
  , CASE(   SELECT cc_value
            FROM   channelconfig cc
            INNER JOIN configuration cfg ON
                    cc_config = configuration_id
                AND configuration_name = 'hedgehog'
            WHERE  cc.cc_channel = c.channel_id)
        WHEN 'false' THEN 0
        WHEN 'true'  THEN 1
        ELSE              0
        END             hedgehog
  , CASE(   SELECT cc_value
            FROM   channelconfig cc
            INNER JOIN configuration cfg ON
                    cc_config = configuration_id
                AND configuration_name = 'autoLink'
            WHERE  cc.cc_channel = c.channel_id)
        WHEN 'false' THEN 0
        WHEN 'true'  THEN 1
        ELSE              0
        END             autolink
  , CASE(   SELECT cc_value
            FROM   channelconfig cc
            INNER JOIN configuration cfg ON
                    cc_config = configuration_id
                AND configuration_name = 'useLongUserInfo'
            WHERE  cc.cc_channel = c.channel_id)
        WHEN 'false' THEN 0
        WHEN 'true'  THEN 1
        ELSE              0
        END             longuserinfo
FROM channel c;");

            var channels = legacyChannels.List();

            foreach (var obj in channels)
            {
                var oldChannel = (object[])obj;

                var oldSiteId = int.Parse((string)oldChannel[4]);
                var baseWiki = lookupCache.ContainsKey(oldSiteId) ? lookupCache[oldSiteId] : null;

                bool enabled = (int)oldChannel[2] == 1;

                bool isSilenced = (long)oldChannel[3] == 1;
                bool hedgehogCurled = (long)oldChannel[5] == 1;
                bool autoLinkEnabled = (long)oldChannel[6] == 1;
                bool useLongUserInfo = (long)oldChannel[7] == 1;

                var newChannel = new Channel
                {
                    Name = (string)oldChannel[0],
                    Password = (string)oldChannel[1],
                    Enabled = enabled,
                    IsSilenced = isSilenced,
                    HedgehogCurled = hedgehogCurled,
                    AutoLinkEnabled = autoLinkEnabled,
                    UseLongUserInfo = useLongUserInfo,
                    BaseWiki = baseWiki
                };

                database.Save(newChannel);
            }
        }

        /// <summary>
        /// The recreate permissions.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        private void RecreatePermissions(ISession database)
        {
            this.logger.Info("Clearing old flag groups and associations...");

            var deleteFlagGroups = database.CreateSQLQuery("DELETE FROM flaggroup");
            deleteFlagGroups.ExecuteUpdate();

            this.logger.Info("Creating new groups");

            var owner = new FlagGroup { IsProtected = true, Name = "Owner", Flags = new List<FlagGroupAssoc>() };
            var ownerFlags = new[] { "D", "O", "A", "P", "S", "B", "C" };
            ownerFlags.Apply(x => owner.Flags.Add(new FlagGroupAssoc { Flag = x, FlagGroup = owner }));
            this.logger.InfoFormat("Creating {0}", owner);
            database.Save(owner);

            var superuser = new FlagGroup { IsProtected = true, Name = "LegacySuperuser", Flags = new List<FlagGroupAssoc>() };
            var superuserFlags = new[] { "A", "P", "S", "B", "C" };
            superuserFlags.Apply(x => superuser.Flags.Add(new FlagGroupAssoc { Flag = x, FlagGroup = superuser }));
            this.logger.InfoFormat("Creating {0}", superuser);
            database.Save(superuser);

            var advanced = new FlagGroup { IsProtected = true, Name = "LegacyAdvanced", Flags = new List<FlagGroupAssoc>() };
            var advancedFlags = new[] { "P", "B", "C" };
            advancedFlags.Apply(x => advanced.Flags.Add(new FlagGroupAssoc { Flag = x, FlagGroup = advanced }));
            this.logger.InfoFormat("Creating {0}", advanced);
            database.Save(advanced);

            var normal = new FlagGroup { IsProtected = true, Name = "LegacyNormal", Flags = new List<FlagGroupAssoc>() };
            var normalFlags = new[] { "B" };
            normalFlags.Apply(x => normal.Flags.Add(new FlagGroupAssoc { Flag = x, FlagGroup = normal }));
            this.logger.InfoFormat("Creating {0}", normal);
            database.Save(normal);

            var stwalkerster = new FlagGroupUser
                                   {
                                       Account = "stwalkerster",
                                       Username = "*",
                                       Nickname = "*",
                                       Hostname = "*",
                                       FlagGroup = owner,
                                       Protected = true
                                   };
            this.logger.InfoFormat("Creating user {0}", stwalkerster);
            database.Save(stwalkerster);

            database.Flush();

            this.logger.InfoFormat("Importing old users");

            var sqlQuery = database.CreateSQLQuery(@"select 
                      replace(user_nickname, '%', '*') nickname
                    , replace(user_username, '%', '*') username
                    , replace(user_hostname, '%', '*') hostname
                    , CASE user_accesslevel
	                    when 'Superuser' then (select id from flaggroup where name = 'LegacySuperuser')
                        when 'Advanced' then (select id from flaggroup where name = 'LegacyAdvanced')
	                    when 'Normal' then (select id from flaggroup where name = 'LegacyNormal')
	                    when 'Developer' then (select id from flaggroup where name = 'Owner')
                      end flaggroup_id
                    from user");

            var groupUsers = sqlQuery.List();

            Dictionary<string, FlagGroup> flagGroups = database.QueryOver<FlagGroup>().List().ToDictionary(x => x.Id.ToString());

            foreach (var obj in groupUsers)
            {
                var oldGroupUser = (object[])obj;

                var groupUser = new FlagGroupUser
                                    {
                                        Nickname = (string)oldGroupUser[0],
                                        Username = (string)oldGroupUser[1],
                                        Hostname = (string)oldGroupUser[2],
                                        Account = "*",
                                        Protected = false,
                                        FlagGroup = flagGroups[(string)oldGroupUser[3]]
                                    };

                database.Save(groupUser);
            }
        }
    }
}