// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupParseFlagsDataSource.cs" company="Helpmebot Development Team">
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
//   The flag group parse flags data source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Tests.TestData
{
    using System.Collections.Generic;

    using Helpmebot.Model;

    using NUnit.Framework;

    /// <summary>
    /// The flag group parse flags data source.
    /// </summary>
    internal class FlagGroupParseFlagsDataSource : TestDataSourceBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="FlagGroupParseFlagsDataSource"/> class.
        /// </summary>
        public FlagGroupParseFlagsDataSource()
        {
            this.testCaseData = new List<TestCaseData>();

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup { Flags = new List<FlagGroupAssoc>() }, 
                    "B", 
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } }, 
                    "C", 
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "C" } } }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } }, 
                    "+C", 
                    new FlagGroup
                        {
                            Flags =
                                new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" }, 
                                        new FlagGroupAssoc { Flag = "C" }
                                    }
                        }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup
                        {
                            Flags =
                                new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" }, 
                                        new FlagGroupAssoc { Flag = "C" }
                                    }
                        }, 
                    "-C", 
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup
                        {
                            Flags =
                                new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" }, 
                                        new FlagGroupAssoc { Flag = "C" }, 
                                        new FlagGroupAssoc { Flag = "D" }
                                    }
                        }, 
                    "-C+EF", 
                    new FlagGroup
                        {
                            Flags =
                                new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" }, 
                                        new FlagGroupAssoc { Flag = "D" }, 
                                        new FlagGroupAssoc { Flag = "E" }, 
                                        new FlagGroupAssoc { Flag = "F" }
                                    }
                        }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup
                        {
                            Flags =
                                new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" }, 
                                        new FlagGroupAssoc { Flag = "C" }, 
                                        new FlagGroupAssoc { Flag = "D" }
                                    }
                        }, 
                    "Z-C+EF", 
                    new FlagGroup
                        {
                            Flags =
                                new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "Z" }, 
                                        new FlagGroupAssoc { Flag = "E" }, 
                                        new FlagGroupAssoc { Flag = "F" }
                                    }
                        }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup(), 
                    "+A", 
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "A" } } }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup(), 
                    "+AAAAAAAA", 
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "A" } } }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup(), 
                    "+A-A+A-A+A-A+A", 
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "A" } } }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } }, 
                    "+A-A+A-A+A-A", 
                    new FlagGroup { Flags = new List<FlagGroupAssoc> { new FlagGroupAssoc { Flag = "B" } } }));

            this.testCaseData.Add(
                new TestCaseData(new FlagGroup(), "-A", new FlagGroup { Flags = new List<FlagGroupAssoc>() }));

            this.testCaseData.Add(
                new TestCaseData(
                    new FlagGroup { Flags = new List<FlagGroupAssoc>() }, 
                    "Bb", 
                    new FlagGroup
                        {
                            Flags =
                                new List<FlagGroupAssoc>
                                    {
                                        new FlagGroupAssoc { Flag = "B" }, 
                                        new FlagGroupAssoc { Flag = "b" }
                                    }
                        }));
        }

        #endregion
    }
}