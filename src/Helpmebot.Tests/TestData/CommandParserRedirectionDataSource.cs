// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandParserRedirectionDataSource.cs" company="Helpmebot Development Team">
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
//   The redirection data source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Tests.TestData
{
    using System.Collections.Generic;

    using NUnit.Framework;

    /// <summary>
    /// The redirection data source.
    /// </summary>
    internal class CommandParserRedirectionDataSource : TestDataSourceBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandParserRedirectionDataSource"/> class. 
        /// </summary>
        public CommandParserRedirectionDataSource()
        {
            this.testCaseData = new List<TestCaseData>
                                    {
                                        new TestCaseData("a b c", "a b c", string.Empty), 
                                        new TestCaseData("a", "a", string.Empty), 
                                        new TestCaseData(">foo", string.Empty, "foo"), 
                                        new TestCaseData("> foo", string.Empty, "foo"), 
                                        new TestCaseData("a >b c", "a c", "b"), 
                                        new TestCaseData("a > b c", "a c", "b"), 
                                        new TestCaseData("a >b >c d", "a d", "b c"), 
                                        new TestCaseData("a >b", "a", "b"), 
                                        new TestCaseData(">a b", "b", "a"), 
                                        new TestCaseData("a > b", "a", "b"), 
                                        new TestCaseData("> a b", "b", "a"), 
                                        new TestCaseData("a> b", "a> b", string.Empty), 
                                        new TestCaseData("a >", "a >", string.Empty), 
                                        new TestCaseData("a b >>>", "a b >>>", string.Empty).Ignore(
                                            "Issue with both legacy and new parsers, plus I'm unsure if this is the correct behaviour."), 
                                    };
        }

        #endregion
    }
}