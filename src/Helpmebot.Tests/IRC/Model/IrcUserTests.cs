// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcUserTests.cs" company="Helpmebot Development Team">
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
//   The irc user tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Tests.IRC.Model
{
    using Helpmebot.IRC.Model;
    using Helpmebot.Tests.TestData;

    using NUnit.Framework;

    /// <summary>
    /// The IRC user tests.
    /// </summary>
    [TestFixture]
    public class IrcUserTests : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The should create from prefix.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="expected">
        /// The expected.
        /// </param>
        [TestCaseSource(typeof(IrcUserTestDataSource))]
        public void ShouldCreateFromPrefix(string input, IrcUser expected)
        {
            // arrange

            // act
            var actual = IrcUser.FromPrefix(input);

            // assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /// <summary>
        /// The should create from prefix.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="expected">
        /// The expected.
        /// </param>
        [TestCaseSource(typeof(IrcUserParseDataSource))]
        public void ShouldParseIntoModel(string input, IrcUser expected)
        {
            // arrange

            // act
            var actual = IrcUser.Parse(input);

            // assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion
    }
}