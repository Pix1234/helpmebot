// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupUserWildcardDataSource.cs" company="Helpmebot Development Team">
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
//   The flag group user wildcard data source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Tests.TestData
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using NUnit.Framework;

    /// <summary>
    /// The flag group user wildcard testCaseData source.
    /// </summary>
    internal class FlagGroupUserWildcardDataSource : TestDataSourceBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="FlagGroupUserWildcardDataSource"/> class.
        /// </summary>
        public FlagGroupUserWildcardDataSource()
        {
            this.testCaseData = new List<TestCaseData>();

            this.testCaseData.Add(new TestCaseData("foo", new Regex("foo")));
            this.testCaseData.Add(new TestCaseData("foo?", new Regex("foo.?")));
            this.testCaseData.Add(new TestCaseData("fo*o", new Regex("fo.*o")));
            this.testCaseData.Add(new TestCaseData("foo|away", new Regex(Regex.Escape("foo|away"))));
        }

        #endregion
    }
}