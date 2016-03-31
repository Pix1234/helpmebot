// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserFlagServiceDataSource.cs" company="Helpmebot Development Team">
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
//   The user flag service data source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.TestData
{
    using System.Collections.Generic;

    using Helpmebot.Model;

    using NUnit.Framework;

    /// <summary>
    /// The user flag service data source.
    /// </summary>
    public class UserFlagServiceDataSource : TestDataSourceBase
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="UserFlagServiceDataSource"/> class.
        /// </summary>
        public UserFlagServiceDataSource()
        {
            this.testCaseData = new List<TestCaseData>();

            var groupA = new FlagGroup { DenyGroup = false, Flags = new[] { new FlagGroupAssoc { Flag = "A" } } };
            
            var groupAAndB = new FlagGroup
                             {
                                 DenyGroup = false,
                                 Flags =
                                     new[]
                                         {
                                             new FlagGroupAssoc { Flag = "A" }, new FlagGroupAssoc { Flag = "B" }
                                         }
                             };

            var groupC = new FlagGroup { DenyGroup = false, Flags = new[] { new FlagGroupAssoc { Flag = "C" } } };
            
            var groupNotA = new FlagGroup { DenyGroup = true, Flags = new[] { new FlagGroupAssoc { Flag = "A" } } };

            var group = new FlagGroup { DenyGroup = false, Flags = new FlagGroupAssoc[] { } };
            var groupN = new FlagGroup { DenyGroup = true, Flags = new FlagGroupAssoc[] { } };

            this.testCaseData.Add(new TestCaseData(new[] { groupA }, new[] { "A" }));
            this.testCaseData.Add(new TestCaseData(new[] { groupA, groupAAndB }, new[] { "A", "B" }));
            this.testCaseData.Add(new TestCaseData(new[] { groupNotA, groupAAndB }, new[] { "B" }));
            this.testCaseData.Add(new TestCaseData(new[] { groupNotA, groupC }, new[] { "C" }));
            this.testCaseData.Add(new TestCaseData(new[] { group }, new string[] { }));
            this.testCaseData.Add(new TestCaseData(new[] { groupN }, new string[] { }));
        }
    }
}