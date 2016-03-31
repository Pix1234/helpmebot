// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WikiLinkServiceTests.cs" company="Helpmebot Development Team">
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
//   The wiki link service tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Helpmebot.Model;
    using Helpmebot.Services;

    using NUnit.Framework;

    /// <summary>
    /// The wiki link service tests.
    /// </summary>
    [TestFixture]
    public class WikiLinkServiceTests : TestBase
    {
        #region Fields

        /// <summary>
        /// The service.
        /// </summary>
        private WikiLinkService service;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The local setup.
        /// </summary>
        public override void LocalSetup()
        {
            base.LocalSetup();

            var interwikiPrefix = new InterwikiPrefix { Prefix = "w", Url = Encoding.UTF8.GetBytes("http://wiki/$1") };
            var enwpPrefix = new InterwikiPrefix { Prefix = "wmf", Url = Encoding.UTF8.GetBytes("https://en.wikipedia.org/wiki/$1") };

            this.service = new WikiLinkService(this.Logger.Object, this.DatabaseSession.Object, interwikiPrefix);
            this.service.InterwikiCache.Add(enwpPrefix.Prefix, enwpPrefix);
        }

        /// <summary>
        /// The test link parser.
        /// </summary>
        /// <param name="inputData">
        /// The input data.
        /// </param>
        /// <param name="expected">
        /// The expected.
        /// </param>
        [TestCase("[[abc]]", new[] { "abc" })]
        [TestCase("[[abc]] [[def]]", new[] { "abc", "def" })]
        [TestCase("[[abc]] def", new[] { "abc" })]
        [TestCase("def [[abc]]", new[] { "abc" })]
        [TestCase("{{abc}}", new[] { "Template:abc" })]
        [TestCase("[[foo]] {{abc}}", new[] { "foo", "Template:abc" })]
        [TestCase("[[foo", new string[0])]
        [TestCase("]]foo[[", new string[0])]
        [TestCase("[[foo[[", new string[0])]
        [TestCase("]]foo]]", new string[0])]
        [TestCase("[[foo}}", new string[0])]
        [TestCase("{{foo]]", new string[0])]
        [TestCase("[[[[foo]]", new[] { "foo" })]
        [TestCase("[[[[foo]]]]", new[] { "foo" })]
        [TestCase("[[foo]]]]", new[] { "foo" })]
        [TestCase("{{[[foo]]}}", new[] { "foo" }, IgnoreReason = "Broken prior to rewrite")]
        [TestCase("[[{{foo}}]]", new[] { "Template:foo" }, IgnoreReason = "Broken prior to rewrite")]
        public void TestLinkParser(string inputData, IEnumerable<string> expected)
        {
            // act
            var links = this.service.ParseForLinks(inputData).ToList();

            // assert
            Assert.That(links, Is.EqualTo(expected));
        }

        /// <summary>
        /// The test linker.
        /// </summary>
        /// <param name="wikilink">
        /// The wikilink.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        [TestCase("Link", "http://wiki/Link")]
        [TestCase("w:Link", "http://wiki/Link")]
        [TestCase("wmf:Link", "https://en.wikipedia.org/wiki/Link")]
        public void TestLinker(string wikilink, string url)
        {
            // act
            var link = this.service.GetLink(wikilink);

            // assert
            Assert.That(link.ToString(), Is.EqualTo(url));
        }

        /// <summary>
        /// The test sanitiser.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="expected">
        /// The expected.
        /// </param>
        [TestCase("foo", "foo")]
        [TestCase("foo bar", "foo_bar")]
        [TestCase("foo(bar)", "foo(bar)")]
        [TestCase("foo,bar", "foo,bar")]
        [TestCase("foo;bar", "foo;bar")]
        [TestCase("foo@bar", "foo@bar")]
        [TestCase("foo$bar", "foo$bar")]
        [TestCase("foo*bar", "foo*bar")]
        [TestCase("foo/bar", "foo/bar")]
        [TestCase("Help:Contents", "Help:Contents")]
        [TestCase("foo&bar", "foo%26bar")]
        [TestCase("foo=bar", "foo%3dbar")]
        [TestCase("foo+bar", "foo%2bbar")]
        [TestCase("foo'bar", "foo%27bar")]
        public void TestSanitiser(string input, string expected)
        {
            // act
            var actual = WikiLinkService.SanitisePageTitle(input);

            // assert
            Assert.That(actual, Is.EqualTo(expected));
        }
        #endregion
    }
}