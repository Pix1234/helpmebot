namespace Helpmebot.Tests.TestData
{
    using System;

    using Helpmebot.IRC.Model;

    using NUnit.Framework;

    /// <summary>
    /// The irc user parse data source.
    /// </summary>
    internal class IrcUserParseDataSource : IrcUserTestDataSource
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="IrcUserParseDataSource"/> class.
        /// </summary>
        public IrcUserParseDataSource()
        {
            this.testCaseData.Add(new TestCaseData("$", null).Throws(typeof(NotSupportedException)));
            this.testCaseData.Add(new TestCaseData("$b", null).Throws(typeof(NotSupportedException)));
            this.testCaseData.Add(new TestCaseData("$a", null).Throws(typeof(NotSupportedException)));
            this.testCaseData.Add(new TestCaseData("$~a", null).Throws(typeof(NotSupportedException)));
            this.testCaseData.Add(new TestCaseData("$~b", null).Throws(typeof(NotSupportedException)));
            this.testCaseData.Add(new TestCaseData("$~a:foo", null).Throws(typeof(NotSupportedException)));
            this.testCaseData.Add(new TestCaseData("$a:foo", new IrcUser { Account = "foo" }));
        }

        #endregion
    }
}