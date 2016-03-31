// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupUserTests.cs" company="Helpmebot Development Team">
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
//   The flag group user tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.Model
{
    using System.Text.RegularExpressions;

    using Helpmebot.Model;
    using Helpmebot.Tests.TestData;

    using NUnit.Framework;

    /// <summary>
    /// The flag group user tests.
    /// </summary>
    [TestFixture]
    public class FlagGroupUserTests : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The test regex escaping.
        /// </summary>
        /// <param name="inputData">
        /// The input Data.
        /// </param>
        /// <param name="expected">
        /// The expected.
        /// </param>
        [TestCaseSource(typeof(FlagGroupUserWildcardDataSource))]
        public void TestRegexEscaping(string inputData, Regex expected)
        {
            // arrange

            // act
            var expressionForWildcards = FlagGroupUser.GetExpressionForWildcards(inputData);

            // assert
            Assert.That(expressionForWildcards.ToString(), Is.EqualTo(expected.ToString()));
        }

        #endregion
    }
}