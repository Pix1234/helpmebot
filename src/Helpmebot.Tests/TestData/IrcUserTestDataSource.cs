// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcUserTestDataSource.cs" company="Helpmebot Development Team">
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
//   The IRC user test data source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Tests.TestData
{
    using System.Collections.Generic;

    using Helpmebot.IRC.Model;

    using NUnit.Framework;

    /// <summary>
    /// The IRC user test data source.
    /// </summary>
    internal class IrcUserTestDataSource : TestDataSourceBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="IrcUserTestDataSource"/> class.
        /// </summary>
        public IrcUserTestDataSource()
        {
            this.testCaseData = new List<TestCaseData>();

            this.testCaseData.Add(
                new TestCaseData(
                    "Yetanotherx|afk!~Yetanothe@mcbouncer.com", 
                    new IrcUser { Hostname = "mcbouncer.com", Username = "~Yetanothe", Nickname = "Yetanotherx|afk" }));
            this.testCaseData.Add(
                new TestCaseData(
                    "stwalkerster@foo.com", 
                    new IrcUser { Hostname = "foo.com", Nickname = "stwalkerster" }));
            this.testCaseData.Add(new TestCaseData("stwalkerster", new IrcUser { Nickname = "stwalkerster" }));
            this.testCaseData.Add(
                new TestCaseData(
                    "nick!user@host", 
                    new IrcUser { Hostname = "host", Username = "user", Nickname = "nick" }));
        }

        #endregion
    }
}