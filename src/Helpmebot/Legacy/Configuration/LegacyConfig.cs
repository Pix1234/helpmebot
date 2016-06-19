// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyConfig.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Legacy.Configuration
{
    using System.Collections.Generic;

    using Castle.Core.Logging;

    using Helpmebot.Legacy.Database;

    using Microsoft.Practices.ServiceLocation;

    using MySql.Data.MySqlClient;

    /// <summary>
    ///     Handles all configuration settings of the bot
    /// </summary>
    internal class LegacyConfig
    {
        #region Static Fields

        /// <summary>
        ///     The _singleton.
        /// </summary>
        private static LegacyConfig singleton;

        #endregion

        #region Fields

        /// <summary>
        ///     The _configuration cache.
        /// </summary>
        private readonly Dictionary<string, ConfigurationSetting> configurationCache;

        /// <summary>
        ///     The legacy database.
        /// </summary>
        private readonly ILegacyDatabase legacyDatabase;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initialises a new instance of the <see cref="LegacyConfig" /> class.
        /// </summary>
        protected LegacyConfig()
        {
            // FIXME: ServiceLocator - Legacy database
            this.legacyDatabase = ServiceLocator.Current.GetInstance<ILegacyDatabase>();
            this.Log = ServiceLocator.Current.GetInstance<ILogger>().CreateChildLogger("LegacyConfig");
            this.legacyDatabase.Connect();

            this.configurationCache = new Dictionary<string, ConfigurationSetting>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        #endregion

        #region Public Indexers

        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified local option.
        /// </summary>
        /// <param name="localOption">
        /// The local Option.
        /// </param>
        /// <param name="locality">
        /// The locality.
        /// </param>
        /// <value>
        /// </value>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string this[string localOption, string locality]
        {
            get
            {
                return this.legacyDatabase.ProcHmbGetLocalOption(localOption, locality);
            }

            set
            {
                this.SetLocalOption(locality, localOption, value);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Singletons this instance.
        /// </summary>
        /// <returns>
        ///     The <see cref="LegacyConfig" />.
        /// </returns>
        public static LegacyConfig Singleton()
        {
            return singleton ?? (singleton = new LegacyConfig());
        }

        /// <summary>
        ///     The clear cache.
        /// </summary>
        public void ClearCache()
        {
            lock (this.configurationCache)
            {
                this.configurationCache.Clear();
            }
        }

        /// <summary>
        /// The get channel id.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetChannelId(string channel)
        {
            var command = new MySqlCommand("SELECT channel_id FROM channel WHERE channel_name = @name;");
            command.Parameters.AddWithValue("@name", channel);

            return this.legacyDatabase.ExecuteScalarSelect(command);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get option id.
        /// </summary>
        /// <param name="optionName">
        /// The option name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetOptionId(string optionName)
        {
            var command =
                new MySqlCommand("SELECT configuration_id FROM `configuration` WHERE configuration_name = @name;");
            command.Parameters.AddWithValue("@name", optionName);

            return this.legacyDatabase.ExecuteScalarSelect(command);
        }

        /// <summary>
        /// The set local option.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="optionName">
        /// The option name.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        private void SetLocalOption(string channel, string optionName, string newValue)
        {
            this.Log.InfoFormat("Setting local option {0} in {1} to {2}", optionName, channel, newValue);

            string channelId = this.GetChannelId(channel);
            
            string configId = this.GetOptionId(optionName);

            this.Log.DebugFormat("Using channelId '{0}' and configId '{1}'", channelId, configId);

            // does setting exist in local table?
            if (newValue == null)
            {
                this.Log.Debug("Deleting local setting");

                var deleteCommand =
                    new MySqlCommand(
                        "DELETE FROM channelconfig WHERE cc_config = @config AND cc_channel = @channel LIMIT 1;");
                deleteCommand.Parameters.AddWithValue("@config", this.GetOptionId(optionName));
                deleteCommand.Parameters.AddWithValue("@channel", this.GetChannelId(channelId));

                this.legacyDatabase.ExecuteCommand(deleteCommand);

                return;
            }

            var selectCommand =
                new MySqlCommand(
                    "SELECT COUNT(*) FROM channelconfig WHERE cc_channel = @channel AND cc_config = @config;");
            selectCommand.Parameters.AddWithValue("@channel", channelId);
            selectCommand.Parameters.AddWithValue("@config", configId);

            string count = this.legacyDatabase.ExecuteScalarSelect(selectCommand);

            if (count == "1")
            {
                this.Log.Debug("Updating local setting");

                // yes: Update
                var command =
                    new MySqlCommand(
                        "UPDATE channelconfig SET cc_value = @value WHERE cc_channel = @channel AND cc_config = @name LIMIT 1;");

                command.Parameters.AddWithValue("@value", newValue);
                command.Parameters.AddWithValue("@name", configId);
                command.Parameters.AddWithValue("@channel", channelId);

                this.legacyDatabase.ExecuteCommand(command);
            }
            else
            {
                this.Log.Debug("inserting local setting");

                // no: Insert
                var command = new MySqlCommand("INSERT INTO channelconfig VALUES ( @channel, @config, @value );");
                command.Parameters.AddWithValue("@channel", channelId);
                command.Parameters.AddWithValue("@config", configId);
                command.Parameters.AddWithValue("@value", newValue);
                this.legacyDatabase.ExecuteCommand(command);
            }
        }

        #endregion
    }
}