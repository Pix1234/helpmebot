// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandParserTests.cs" company="Helpmebot Development Team">
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
//   The command parser tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Tests.Services
{
    using System;

    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Services;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Tests.TestData;
    using Helpmebot.TypedFactories;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// The command parser tests.
    /// </summary>
    [TestFixture]
    public class CommandParserTests : TestBase
    {
        #region Fields

        /// <summary>
        /// The factory.
        /// </summary>
        private readonly Mock<ICommandTypedFactory> factory = new Mock<ICommandTypedFactory>();

        /// <summary>
        /// The IRC client.
        /// </summary>
        private readonly Mock<IIrcClient> ircClient = new Mock<IIrcClient>();

        /// <summary>
        /// The keyword factory.
        /// </summary>
        private readonly Mock<IKeywordCommandFactory> keywordFactory = new Mock<IKeywordCommandFactory>();

        /// <summary>
        /// The keyword service.
        /// </summary>
        private readonly Mock<IKeywordService> keywordService = new Mock<IKeywordService>();

        /// <summary>
        /// The command parser.
        /// </summary>
        private CommandParser commandParser;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The local setup.
        /// </summary>
        public override void LocalSetup()
        {
            this.ircClient.SetupProperty(client => client.Nickname, "BotNickname");
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldFailParseMessage1()
        {
            // arrange
            const string Test = "command arg one two three";

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.Null);
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldFailParseMessage2()
        {
            // arrange
            const string Test = "command";

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.Null);
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldFailParseMessage3()
        {
            // act
            var commandMessage = this.commandParser.ParseCommandMessage(string.Empty, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.Null);
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldFailParseMessage4()
        {
            // arrange
            const string Test = "!";

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.Null);
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldFailParseMessage5()
        {
            // arrange
            const string Test = "BotNickname";

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.Null);
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldFailParseMessage6()
        {
            // arrange
            const string Test = "BotNickname:";

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.Null);
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldFailParseMessage7()
        {
            // arrange
            const string Test = "BotNickname,";

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.Null);
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseCorrectlyWithAltTrigger()
        {
            // arrange
            this.commandParser = new CommandParser(
                "~", 
                this.factory.Object, 
                this.keywordService.Object, 
                this.keywordFactory.Object, 
                this.Logger.Object,
                this.DatabaseSession.Object);
            const string Test = "~command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = false
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly1()
        {
            // arrange
            const string Test = "!BotNickname command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly10()
        {
            // arrange
            const string Test = "!command";
            var message = new CommandMessage
                              {
                                  ArgumentList = string.Empty, 
                                  CommandName = "command", 
                                  OverrideSilence = false
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly11()
        {
            // arrange
            const string Test = "!BotNickname command";
            var message = new CommandMessage
                              {
                                  ArgumentList = string.Empty, 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly1L()
        {
            // arrange
            const string Test = "!botnickname command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly2()
        {
            // arrange
            const string Test = "!command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = false
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly3()
        {
            // arrange
            const string Test = "BotNickname command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly3L()
        {
            // arrange
            const string Test = "botnickname command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly4()
        {
            // arrange
            const string Test = "BotNickname: command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly4L()
        {
            // arrange
            const string Test = "botnickname: command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly5()
        {
            // arrange
            const string Test = "BotNickname, command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly5L()
        {
            // arrange
            const string Test = "botnickname, command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly6()
        {
            // arrange
            const string Test = "BotNickname:command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly6L()
        {
            // arrange
            const string Test = "botnickname:command arg one two three";
            var message = new CommandMessage
                              {
                                  ArgumentList = "arg one two three", 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly7()
        {
            // arrange
            const string Test = "BotNickname:command";
            var message = new CommandMessage
                              {
                                  ArgumentList = string.Empty, 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly7L()
        {
            // arrange
            const string Test = "botnickname:command";
            var message = new CommandMessage
                              {
                                  ArgumentList = string.Empty, 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly8()
        {
            // arrange
            const string Test = "BotNickname: command";
            var message = new CommandMessage
                              {
                                  ArgumentList = string.Empty, 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The should parse message correctly 1.
        /// </summary>
        [Test]
        public void ShouldParseMessageCorrectly9()
        {
            // arrange
            const string Test = "BotNickname command";
            var message = new CommandMessage
                              {
                                  ArgumentList = string.Empty, 
                                  CommandName = "command", 
                                  OverrideSilence = true
                              };

            // act
            var commandMessage = this.commandParser.ParseCommandMessage(Test, "BotNickname");

            // assert
            Assert.That(commandMessage, Is.EqualTo(message));
        }

        /// <summary>
        /// The test find redirection.
        /// </summary>
        /// <param name="inputdata">
        /// The input data.
        /// </param>
        /// <param name="expecteddata">
        /// The expected data.
        /// </param>
        /// <param name="expectedRedir">
        /// The expected redirection.
        /// </param>
        [TestCaseSource(typeof(CommandParserRedirectionDataSource))]
        public void TestFindRedirection(string inputdata, string expecteddata, string expectedRedir)
        {
            // arrange
            string[] input = inputdata.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // act
            var result = this.commandParser.ParseRedirection(input);

            // assert
            Assert.That(
                result.Target, 
                Is.EqualTo(expectedRedir.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
        }

        /// <summary>
        /// The test redirection result.
        /// </summary>
        /// <param name="inputdata">
        /// The input data.
        /// </param>
        /// <param name="expecteddata">
        /// The expected data.
        /// </param>
        /// <param name="expectedRedir">
        /// The expected redirection.
        /// </param>
        [TestCaseSource(typeof(CommandParserRedirectionDataSource))]
        public void TestPostRedirectionMessage(string inputdata, string expecteddata, string expectedRedir)
        {
            // arrange
            string[] input = inputdata.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] expected = expecteddata.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // act
            var result = this.commandParser.ParseRedirection(input);

            // assert
            Assert.That(result.Arguments, Is.EqualTo(expected));
        }

        /// <summary>
        /// The test setup.
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            this.commandParser = new CommandParser(
                "!", 
                this.factory.Object, 
                this.keywordService.Object, 
                this.keywordFactory.Object, 
                this.Logger.Object,
                this.DatabaseSession.Object);
        }

        #endregion
    }
}