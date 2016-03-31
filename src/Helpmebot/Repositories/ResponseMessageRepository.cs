// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseMessageRepository.cs" company="Helpmebot Development Team">
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
//   The message response repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories
{
    using System.Data;

    using Castle.Core.Logging;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using NHibernate;

    /// <summary>
    /// The message response repository.
    /// </summary>
    public class ResponseMessageRepository : IResponseMessageRepository
    {
        /// <summary>
        /// The command parameter.
        /// </summary>
        private readonly IDbDataParameter commandParameter;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The retrieval command.
        /// </summary>
        private readonly IDbCommand retrievalCommand;

        /// <summary>
        /// The session.
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initialises a new instance of the <see cref="ResponseMessageRepository"/> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public ResponseMessageRepository(ISession session, ILogger logger)
        {
            this.logger = logger;
            this.session = session;

            this.retrievalCommand = this.session.Connection.CreateCommand();
            this.retrievalCommand.CommandText =
                "SELECT message_id, message_name, message_text FROM messages WHERE message_name = @name";

            this.commandParameter = this.retrievalCommand.CreateParameter();
            this.commandParameter.ParameterName = "@name";
            this.commandParameter.DbType = DbType.AnsiString;

            this.retrievalCommand.Parameters.Add(this.commandParameter);
        }

        /// <summary>
        /// The get by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="ResponseMessage"/>.
        /// </returns>
        public ResponseMessage GetByName(string name)
        {
            this.logger.DebugFormat("Running db command for message {0}", name);

            this.commandParameter.Value = name;

            lock (this.session.Connection)
            {
                IDataReader reader = null;
                try
                {
                    this.logger.DebugFormat("Opening reader", name);
                    reader = this.retrievalCommand.ExecuteReader(CommandBehavior.SingleRow);

                    var success = reader.Read();

                    if (!success)
                    {
                        this.logger.DebugFormat("Message {0} not found", name);
                        return null;
                    }

                    var messageId = reader.GetInt32(0);
                    var messageName = reader.GetString(1);
                    var messageText = reader.GetString(2);

                    this.logger.DebugFormat("Message {0} found", name);
                    return new ResponseMessage { Id = messageId, Name = messageName, Text = messageText };
                }
                finally
                {
                    if (reader != null && !reader.IsClosed)
                    {
                        this.logger.DebugFormat("Closing reader", name);
                        reader.Close();
                    }
                }
            }
        }
    }
}