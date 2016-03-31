// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupCommandTests.cs" company="Helpmebot Development Team">
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
//   The flag group command tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Tests.Commands.AccessControl
{
    using Helpmebot.Commands.AccessControl;
    using Helpmebot.Model;
    using Helpmebot.Tests.TestData;

    using NUnit.Framework;

    /// <summary>
    /// The flag group command tests.
    /// </summary>
    [TestFixture]
    public class FlagGroupCommandTests : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The test parse flags.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="expected">
        /// The expected.
        /// </param>
        [TestCaseSource(typeof(FlagGroupParseFlagsDataSource))]
        public void TestParseFlags(FlagGroup input, string flags, FlagGroup expected)
        {
            // act
            FlagGroupCommand.ParseFlags(flags, input);

            // assert
            Assert.That(input.ToString(), Is.EqualTo(expected.ToString()));
        }

        #endregion
    }
}