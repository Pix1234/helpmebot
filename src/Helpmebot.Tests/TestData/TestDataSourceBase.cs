// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataSourceBase.cs" company="Helpmebot Development Team">
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
//   The test data source base.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.TestData
{
    using System.Collections;
    using System.Collections.Generic;

    using NUnit.Framework;

    /// <summary>
    /// The test data source base.
    /// </summary>
    public class TestDataSourceBase : IEnumerable<TestCaseData>
    {
        #region Fields

        /// <summary>
        /// The test case data.
        /// </summary>
        protected List<TestCaseData> testCaseData;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            return this.testCaseData.GetEnumerator();
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}