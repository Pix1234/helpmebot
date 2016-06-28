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
    ///     The client application.
    /// </summary>
    public class ClientApplication : IApplication
    {
        /// <summary>
        ///     The database session factory.
        /// </summary>
        private readonly ISessionFactory databaseSessionFactory;

        /// <summary>
        ///     The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientApplication" /> class.
        /// </summary>
        /// <param name="logger">
        ///     The logger.
        /// </param>
        /// <param name="databaseSessionFactory">
        ///     The database session Factory.
        /// </param>
        public ClientApplication(ILogger logger, ISessionFactory databaseSessionFactory)
        {
            this.logger = logger;
            this.databaseSessionFactory = databaseSessionFactory;
        }

        /// <summary>
        ///     The run.
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
                Console.WriteLine("   1: Apply v6->v7 schema updates");
                Console.WriteLine("   2: Recreate permissions");
                Console.WriteLine("   3: Migrate sites and channels");
                Console.WriteLine("   4: Migrate category watchers");
                Console.WriteLine("   9: Apply v6 schema cleanup operations");
                Console.WriteLine();
                Console.Write("Choice [0]: ");
                var readLine = Console.ReadLine();

                if (string.IsNullOrEmpty(readLine) || readLine == "0")
                {
                    break;
                }

                if (readLine == "1")
                {
                    this.DoSchemaChanges(this.DoSchemaUpdates);
                }

                if (readLine == "2")
                {
                    this.DoTransactionally(this.RecreatePermissions);
                }

                if (readLine == "3")
                {
                    this.DoTransactionally(this.MigrateSitesAndChannels);
                }

                if (readLine == "4")
                {
                    this.DoTransactionally(this.MigrateCategoryWatchers);
                }

                if (readLine == "9")
                {
                    this.DoSchemaChanges(this.DoFinalSchemaUpdates);
                }

                Console.Clear();
            }
        }

        /// <summary>
        ///     The do final schema updates.
        /// </summary>
        /// <param name="database">
        ///     The database.
        /// </param>
        private void DoFinalSchemaUpdates(ISession database)
        {
            this.DoSingleSchemaChange("DROP TABLE command;", database);
            this.DoSingleSchemaChange("DROP TABLE user;", database);
            this.DoSingleSchemaChange("DROP TABLE channel_old;", database);
            this.DoSingleSchemaChange("DROP TABLE site;", database);
            this.DoSingleSchemaChange("DROP TABLE ircnetwork;", database);
            this.DoSingleSchemaChange("DROP TABLE channelwatchers;", database);
            this.DoSingleSchemaChange("DROP TABLE watcher;", database);
            this.DoSingleSchemaChange("DROP TABLE channelconfig;", database);
            this.DoSingleSchemaChange("DROP TABLE configuration;", database);
            this.DoSingleSchemaChange("DROP TABLE categoryitems;", database);
        }

        /// <summary>
        ///     The do schema changes.
        /// </summary>
        /// <param name="action">
        ///     The action.
        /// </param>
        private void DoSchemaChanges(Action<ISession> action)
        {
            this.logger.InfoFormat("Opening database connection");
            var database = this.databaseSessionFactory.OpenSession();

            try
            {
                action(database);

                database.Close();
            }
            catch (Exception e)
            {
                this.logger.Error(e.Message, e);
                database.Close();
            }

            this.logger.Info("Done. Press a key to continue.");
            Console.ReadLine();
        }

        /// <summary>
        ///     The do schema updates.
        /// </summary>
        /// <param name="database">
        ///     The database.
        /// </param>
        private void DoSchemaUpdates(ISession database)
        {
            this.DoSingleSchemaChange("alter table channel rename to channel_old;", database);

            this.DoSingleSchemaChange(
                "ALTER TABLE ignoredpages ADD COLUMN id CHAR(36) NOT NULL FIRST;", 
                database);
            this.DoSingleSchemaChange("UPDATE ignoredpages SET id = uuid();", database);
            this.DoSingleSchemaChange(
                @"ALTER TABLE ignoredpages
                      DROP PRIMARY KEY,
                      DROP COLUMN ip_id,
                      CHANGE COLUMN ip_title title VARCHAR(255) NOT NULL,
                      CHANGE COLUMN id id CHAR(36) NOT NULL PRIMARY KEY; ", 
                database);

            this.DoSingleSchemaChange(
                @"ALTER TABLE accesslog
                    CHANGE COLUMN al_accesslevel al_accesslevel VARCHAR(45) NOT NULL ,
                    CHANGE COLUMN al_reqaccesslevel al_reqaccesslevel VARCHAR(45) NOT NULL ,
                    ADD COLUMN account VARCHAR(45) NULL DEFAULT NULL AFTER al_nuh;", 
                    database);

            this.DoSingleSchemaChange(
                @"CREATE TABLE IF NOT EXISTS mediawikisite (
                      id CHAR(36) NOT NULL,
                      api VARCHAR(255) NOT NULL,
                      username VARCHAR(255) NULL DEFAULT NULL,
                      password VARCHAR(255) NULL DEFAULT NULL,
                      name VARCHAR(45) NOT NULL,
                      PRIMARY KEY (id))
                    ENGINE = InnoDB;", 
                    database);

            this.DoSingleSchemaChange(
                @"CREATE TABLE IF NOT EXISTS channelnew (
                      id CHAR(36) NOT NULL,
                      name VARCHAR(30) NOT NULL,
                      password VARCHAR(255) NULL DEFAULT NULL,
                      enabled INT(11) NOT NULL DEFAULT '1',
                      silence INT(11) NOT NULL DEFAULT '0',
                      basewiki CHAR(36) NULL DEFAULT NULL,
                      hedgehog INT(11) NOT NULL DEFAULT '0',
                      autolink INT(11) NOT NULL DEFAULT '0',
                      longuserinfo INT(11) NOT NULL DEFAULT '0',
                      PRIMARY KEY (id),
                      UNIQUE INDEX name_UNIQUE (name ASC),
                      INDEX fk_channelnew_mediawikisite1_idx (basewiki ASC),
                      CONSTRAINT fk_channelnew_mediawikisite1
                        FOREIGN KEY (basewiki)
                        REFERENCES mediawikisite (id)
                        ON DELETE SET NULL
                        ON UPDATE CASCADE)
                    ENGINE = InnoDB;", 
                    database);

            this.DoSingleSchemaChange(
                @"CREATE TABLE IF NOT EXISTS flaggroup (
                      id CHAR(36) NOT NULL,
                      name VARCHAR(45) NOT NULL,
                      denygroup INT(11) NOT NULL DEFAULT '0',
                      protected INT(11) NOT NULL DEFAULT '0',
                      lastmodified TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                      PRIMARY KEY (id),
                      UNIQUE INDEX name_UNIQUE (name ASC))
                    ENGINE = InnoDB;", 
                database);

            this.DoSingleSchemaChange(
                @"CREATE TABLE IF NOT EXISTS flaggroup_assoc (
                      id CHAR(36) NOT NULL,
                      flaggroup_id CHAR(36) NOT NULL,
                      flag VARCHAR(45) NOT NULL,
                      lastmodified TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                      PRIMARY KEY (id),
                      UNIQUE INDEX assoc_unique (flaggroup_id ASC, flag ASC),
                      CONSTRAINT FK_flaggroup
                        FOREIGN KEY (flaggroup_id)
                        REFERENCES flaggroup (id)
                        ON DELETE CASCADE)
                    ENGINE = InnoDB;", 
                database);

            this.DoSingleSchemaChange(
                @"CREATE TABLE IF NOT EXISTS flaggroup_channel (
                      id CHAR(36) NOT NULL,
                      flaggroup_id CHAR(36) NOT NULL,
                      channel_id CHAR(36) NOT NULL,
                      protected INT(1) NOT NULL DEFAULT '0',
                      lastmodified TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                      PRIMARY KEY (id),
                      UNIQUE INDEX flaggroup_channel_UNIQUE (flaggroup_id ASC, channel_id ASC),
                      INDEX fk_flaggroup_channel_channelnew1_idx (channel_id ASC),
                      CONSTRAINT fk_flaggroup_channel_channelnew1
                        FOREIGN KEY (channel_id)
                        REFERENCES channelnew (id)
                        ON DELETE NO ACTION
                        ON UPDATE NO ACTION,
                      CONSTRAINT fk_flaggroup_channel_flaggroup1
                        FOREIGN KEY (flaggroup_id)
                        REFERENCES flaggroup (id)
                        ON DELETE NO ACTION
                        ON UPDATE NO ACTION)
                    ENGINE = InnoDB;",
                database);

            this.DoSingleSchemaChange(
                @"CREATE TABLE IF NOT EXISTS flaggroup_user (
                      id CHAR(36) NOT NULL,
                      flaggroup_id CHAR(36) NOT NULL,
                      nickname VARCHAR(255) NOT NULL,
                      username VARCHAR(255) NOT NULL,
                      hostname VARCHAR(255) NOT NULL,
                      account VARCHAR(45) NOT NULL,
                      protected INT(1) NOT NULL DEFAULT '0',
                      lastmodified TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                      PRIMARY KEY (id),
                      INDEX FK_flaggroup_idx (flaggroup_id ASC),
                      CONSTRAINT FK_flaggroup_user
                        FOREIGN KEY (flaggroup_id)
                        REFERENCES flaggroup (id)
                        ON DELETE CASCADE
                        ON UPDATE CASCADE)
                    ENGINE = InnoDB;", 
                database);

            this.DoSingleSchemaChange(
                @"CREATE TABLE categorywatcher (
                    id CHAR(36) PRIMARY KEY NOT NULL,
                    channel CHAR(36) NOT NULL,
                    mediawikisite CHAR(36) NOT NULL,
                    category VARCHAR(255) NOT NULL,
                    priority INT DEFAULT 1 NOT NULL,
                    keyword VARCHAR(20) NOT NULL,
                    sleeptime INT DEFAULT 20 NOT NULL,
                    showwikilink INT DEFAULT 1 NOT NULL,
                    showshorturl INT DEFAULT 1 NOT NULL,
                    showwaittime INT DEFAULT 1 NOT NULL,
                    minwaittime INT DEFAULT 60 NOT NULL,
                    delta INT DEFAULT 0 NOT NULL,
                    enabled INT DEFAULT 1 NOT NULL,
                    CONSTRAINT categorywatcher_channel_id_fk FOREIGN KEY (channel) REFERENCES channel (id) ON DELETE CASCADE ON UPDATE CASCADE,
                    CONSTRAINT categorywatcher_mediawikisite_id_fk FOREIGN KEY (mediawikisite) REFERENCES mediawikisite (id)
                ) ENGINE=InnoDB;",
                database);

            this.DoSingleSchemaChange(
                "CREATE UNIQUE INDEX categorywatcher_category_uindex ON helpmebot_devel.categorywatcher(channel, category, mediawikisite);",
                database);

            this.DoSingleSchemaChange(
                "CREATE UNIQUE INDEX categorywatcher_keyword_uindex ON helpmebot_devel.categorywatcher(channel, keyword)",
                database);

            this.DoSingleSchemaChange(
                @"CREATE TABLE categorywatcher_items
                    (
                        id CHAR(36) PRIMARY KEY NOT NULL,
                        categorywatcher CHAR(36) NOT NULL,
                        touched TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                        title VARCHAR(255) NOT NULL,
                        CONSTRAINT categorywatcher_items_categorywatcher_id_fk FOREIGN KEY (categorywatcher) REFERENCES categorywatcher (id) ON DELETE CASCADE ON UPDATE CASCADE
                    );",
                database);

            this.DoSingleSchemaChange(
                "CREATE UNIQUE INDEX categorywatcheritems_watchertitle_uindex ON categorywatcher_items (categorywatcher, title);", 
                database);

            this.DoSingleSchemaChange(@"ALTER TABLE mediawikisite ADD articlepath VARCHAR(255) NULL;", database);

            this.DoSingleSchemaChange(
                @"ALTER TABLE categorywatcher ADD itemSingular VARCHAR(255) DEFAULT 'page' NOT NULL;",
                database);

            this.DoSingleSchemaChange(
                @"ALTER TABLE categorywatcher ADD itemPlural VARCHAR(255) DEFAULT 'pages' NOT NULL;",
                database);

            this.DoSingleSchemaChange(
                @"ALTER TABLE categorywatcher ADD itemAction VARCHAR(255) DEFAULT 'in category {0}' NOT NULL",
                database);
        }

        /// <summary>
        ///     The do single schema change.
        /// </summary>
        /// <param name="command">
        ///     The command.
        /// </param>
        /// <param name="database">
        ///     The database.
        /// </param>
        private void DoSingleSchemaChange(string command, ISession database)
        {
            this.logger.InfoFormat("Running: {0}", command);
            database.CreateSQLQuery(command).ExecuteUpdate();
        }

        /// <summary>
        ///     do something in a transaction.
        /// </summary>
        /// <param name="action">
        ///     The action.
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
        ///     The migrate channels.
        /// </summary>
        /// <param name="database">
        ///     The database.
        /// </param>
        private void MigrateSitesAndChannels(ISession database)
        {
            this.logger.Info("Clearing old sites and channels");

            var deleteChannels = database.CreateSQLQuery("DELETE FROM channel");
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
                                      Name = (string)oldSite[4]
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
FROM channel_old c;");

            var channels = legacyChannels.List();

            foreach (var obj in channels)
            {
                var oldChannel = (object[])obj;

                var oldSiteId = int.Parse((string)oldChannel[4]);
                var baseWiki = lookupCache.ContainsKey(oldSiteId) ? lookupCache[oldSiteId] : null;

                var enabled = (int)oldChannel[2] == 1;

                var isSilenced = (long)oldChannel[3] == 1;
                var hedgehogCurled = (long)oldChannel[5] == 1;
                var autoLinkEnabled = (long)oldChannel[6] == 1;
                var useLongUserInfo = (long)oldChannel[7] == 1;

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
        ///     The recreate permissions.
        /// </summary>
        /// <param name="database">
        ///     The database.
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

            var superuser = new FlagGroup
                                {
                                    IsProtected = true, 
                                    Name = "LegacySuperuser", 
                                    Flags = new List<FlagGroupAssoc>()
                                };
            var superuserFlags = new[] { "A", "P", "S", "B", "C" };
            superuserFlags.Apply(x => superuser.Flags.Add(new FlagGroupAssoc { Flag = x, FlagGroup = superuser }));
            this.logger.InfoFormat("Creating {0}", superuser);
            database.Save(superuser);

            var advanced = new FlagGroup
                               {
                                   IsProtected = true, 
                                   Name = "LegacyAdvanced", 
                                   Flags = new List<FlagGroupAssoc>()
                               };
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

            var flagGroups = database.QueryOver<FlagGroup>().List().ToDictionary(x => x.Id.ToString());

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

        /// <summary>
        /// The migrate category watchers.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        private void MigrateCategoryWatchers(ISession database)
        {
            var queryString = @"
                INSERT INTO categorywatcher
                SELECT
                  uuid()                                AS id,
                  c.id                                  AS channel,
                  c.basewiki                            AS mediawikisite,
                  w.watcher_category                    AS category,
                  w.watcher_priority                    AS priority,
                  w.watcher_keyword                     AS keyword,
                  cast(w.watcher_sleeptime / 60 AS INT) AS sleeptime,
                  1                                     AS showwikilink,
                  CASE (
                    SELECT cc_value
                    FROM channelconfig cc
                      INNER JOIN configuration cfg
                        ON cc_config = configuration_id AND configuration_name = 'useShortUrlsInsteadOfWikilinks'
                    WHERE cc.cc_channel = co.channel_id)
                    WHEN 'false' THEN 0
                    WHEN 'true' THEN 1
                    ELSE 0
                  END                                   AS showshorturl,
                  CASE (
                    SELECT cc_value
                    FROM channelconfig cc
                      INNER JOIN configuration cfg 
                        ON cc_config = configuration_id AND configuration_name = 'showWaitTime'
                    WHERE cc.cc_channel = co.channel_id)
                    WHEN 'false' THEN 0
                    WHEN 'true' THEN 1
                    ELSE 0
                  END                                   AS showwaittime,
                  60                                    AS minwaittime,
                  0                                     AS delta,
                  CASE
                    WHEN c.enabled = 0 THEN 0
                    ELSE 1
                  END                                   AS enabled
                FROM watcher w
                  JOIN channelwatchers cw ON cw_watcher = w.watcher_id
                  LEFT JOIN channel_old co ON co.channel_id = cw.cw_channel
                  LEFT JOIN channel c ON co.channel_name = c.name
                ORDER BY watcher_id;";

            database.CreateSQLQuery(queryString).ExecuteUpdate();
        }
    }
}