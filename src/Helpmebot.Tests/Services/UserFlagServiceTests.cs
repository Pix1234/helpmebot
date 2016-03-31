// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserFlagServiceTests.cs" company="Helpmebot Development Team">
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
//   The user flag service tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Tests.Services
{
    using System.Collections.Generic;

    using Helpmebot.Model;
    using Helpmebot.Services;
    using Helpmebot.Tests.TestData;

    using NUnit.Framework;

    /// <summary>
    /// The user flag service tests.
    /// </summary>
    public class UserFlagServiceTests : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The test flag parser.
        /// </summary>
        /// <param name="flagGroups">
        /// The flag groups.
        /// </param>
        /// <param name="expected">
        /// The expected.
        /// </param>
        [TestCaseSource(typeof(UserFlagServiceDataSource))]
        public void TestFlagParser(IEnumerable<FlagGroup> flagGroups, IEnumerable<string> expected)
        {
            // arrange

            // act
            var result = UserFlagService.GetFlagsFromGroups(flagGroups);

            // assert
            Assert.That(expected, Is.EqualTo(result));
        }

        #endregion
    }
}