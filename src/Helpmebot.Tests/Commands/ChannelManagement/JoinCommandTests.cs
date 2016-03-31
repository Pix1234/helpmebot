// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JoinCommandTests.cs" company="Helpmebot Development Team">
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
//   The join command tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Tests.Commands.ChannelManagement
{
    using System;
    using System.Data;
    using System.Linq.Expressions;

    using Castle.Core.Internal;

    using Helpmebot.Commands.ChannelManagement;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.IRC.Messages;
    using Helpmebot.IRC.Model;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using Moq;

    using NHibernate;

    using NUnit.Framework;

    /// <summary>
    /// The join command tests.
    /// </summary>
    [TestFixture]
    public class JoinCommandTests : TestBase
    {
        /// <summary>
        /// The client mock.
        /// </summary>
        private Mock<IIrcClient> clientMock;

        /// <summary>
        /// The command service helper mock.
        /// </summary>
        private Mock<ICommandServiceHelper> commandServiceHelperMock;

        /// <summary>
        /// The channel.
        /// </summary>
        private Channel channel;

        /// <summary>
        /// The channel 2.
        /// </summary>
        private Channel channel2;

        /// <summary>
        /// The test setup.
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            this.DatabaseSession = new Mock<ISession>();

            this.channel = new Channel { Enabled = false, Name = "##test" };
            this.channel2 = new Channel { Enabled = false, Name = "##test2" };
            
            this.clientMock = new Mock<IIrcClient>();
            this.clientMock.SetupGet(x => x.Nickname).Returns("HMBDebug");

            var userflagservice = new Mock<IUserFlagService>();
            userflagservice.Setup(x => x.GetFlagsForUser(It.IsAny<IUser>())).Returns(new[] { Flag.LegacySuperuser });

            var accesslogservice = new Mock<IAccessLogService>();

            var channelrepository = new Mock<IChannelRepository>();
            channelrepository.Setup(x => x.GetByName(It.IsAny<string>(), It.IsAny<ISession>())).Returns(
                (string x, ISession y) =>
                    {
                        if (x == this.channel.Name)
                        {
                            return this.channel;
                        }

                        if (x == this.channel2.Name)
                        {
                            return this.channel2;
                        }

                        return null;
                    });

            this.commandServiceHelperMock = new Mock<ICommandServiceHelper>();
            this.commandServiceHelperMock.SetupGet(x => x.ConfigurationHelper).Returns(this.ConfigurationHelper.Object);
            this.commandServiceHelperMock.SetupGet(x => x.UserFlagService).Returns(userflagservice.Object);
            this.commandServiceHelperMock.SetupGet(x => x.AccessLogService).Returns(accesslogservice.Object);
            this.commandServiceHelperMock.SetupGet(x => x.Client).Returns(this.clientMock.Object);
            this.commandServiceHelperMock.SetupGet(x => x.ChannelRepository).Returns(channelrepository.Object);
            
            var transaction = new Mock<ITransaction>();
            transaction.SetupGet(x => x.IsActive).Returns(true);
            
            this.DatabaseSession.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(transaction.Object);
        }

        /// <summary>
        /// The test successful join.
        /// </summary>
        [Test]
        public void TestSuccessfulJoin()
        {
            var command = new JoinCommand(
                "##test", 
                new IrcUser { Account = "test", Hostname = "test", Nickname = "test", Username = "test" }, 
                new[] { "##test" }, 
                this.Logger.Object, 
                this.DatabaseSession.Object, 
                this.commandServiceHelperMock.Object);

            var result = command.Run();
            Assert.That(result.IsNullOrEmpty());

            var message = new Mock<IMessage>();
            message.SetupGet(x => x.Command).Returns("JOIN");
            message.SetupGet(x => x.Parameters)
                .Returns(new[] { "##test", "HMBDebug", "Helpmebot (helpmebot@helpmebot.org.uk) - IRC bot" });

            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Once());

            this.clientMock.Raise(
                x => x.ReceivedMessage += null, 
                new MessageReceivedEventArgs(message.Object, this.clientMock.Object));

            Assert.That(command.Executed);
            Assert.That(this.channel.Enabled, Is.True);
        }

        [Test]
        public void TestForwardJoin()
        {
            var command = new JoinCommand(
                "##test",
                new IrcUser { Account = "test", Hostname = "test", Nickname = "test", Username = "test" },
                new[] { "##test" },
                this.Logger.Object,
                this.DatabaseSession.Object,
                this.commandServiceHelperMock.Object);

            var result = command.Run();
            Assert.That(result.IsNullOrEmpty());
            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Once());
            Assert.That(this.channel.Enabled, Is.True);

            var message = new Mock<IMessage>();
            message.SetupGet(x => x.Command).Returns("470");
            message.SetupGet(x => x.Parameters)
                .Returns(
                    new[] { "HMBDebug", this.channel.Name, this.channel2.Name, "Forwarding to another channel" });
            
            this.clientMock.Raise(
                x => x.ReceivedMessage += null,
                new MessageReceivedEventArgs(message.Object, this.clientMock.Object));

            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Exactly(2));
            this.DatabaseSession.Verify(x => x.Save(this.channel2), Times.Once());

            Assert.That(this.channel.Enabled, Is.False);
            Assert.That(this.channel2.Enabled, Is.True);
        }

        [Test]
        public void TestChannelFullJoin()
        {
            var command = new JoinCommand(
                "##test",
                new IrcUser { Account = "test", Hostname = "test", Nickname = "test", Username = "test" },
                new[] { "##test" },
                this.Logger.Object,
                this.DatabaseSession.Object,
                this.commandServiceHelperMock.Object);

            var result = command.Run();
            Assert.That(result.IsNullOrEmpty());
            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Once());
            Assert.That(this.channel.Enabled, Is.True);

            var message = new Mock<IMessage>();
            message.SetupGet(x => x.Command).Returns("471");
            message.SetupGet(x => x.Parameters)
                .Returns(
                    new[] { "HMBDebug", this.channel.Name, "Cannot join channel (+l) - channel is full, try again later" });

            this.clientMock.Raise(
                x => x.ReceivedMessage += null,
                new MessageReceivedEventArgs(message.Object, this.clientMock.Object));

            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Exactly(1));
            Assert.That(this.channel.Enabled, Is.True);
        }

        [Test]
        public void TestInviteOnlyJoin()
        {
            var command = new JoinCommand(
                "##test",
                new IrcUser { Account = "test", Hostname = "test", Nickname = "test", Username = "test" },
                new[] { "##test" },
                this.Logger.Object,
                this.DatabaseSession.Object,
                this.commandServiceHelperMock.Object);

            var result = command.Run();
            Assert.That(result.IsNullOrEmpty());
            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Once());
            Assert.That(this.channel.Enabled, Is.True);

            var message = new Mock<IMessage>();
            message.SetupGet(x => x.Command).Returns("473");
            message.SetupGet(x => x.Parameters)
                .Returns(
                    new[] { "HMBDebug", this.channel.Name, "Cannot join channel (+i) - you must be invited" });

            this.clientMock.Raise(
                x => x.ReceivedMessage += null,
                new MessageReceivedEventArgs(message.Object, this.clientMock.Object));

            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Exactly(2));
            Assert.That(this.channel.Enabled, Is.False);
        }

        [Test]
        public void TestBannedJoin()
        {
            var command = new JoinCommand(
                "##test",
                new IrcUser { Account = "test", Hostname = "test", Nickname = "test", Username = "test" },
                new[] { "##test" },
                this.Logger.Object,
                this.DatabaseSession.Object,
                this.commandServiceHelperMock.Object);

            var result = command.Run();
            Assert.That(result.IsNullOrEmpty());
            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Once());
            Assert.That(this.channel.Enabled, Is.True);

            var message = new Mock<IMessage>();
            message.SetupGet(x => x.Command).Returns("474");
            message.SetupGet(x => x.Parameters)
                .Returns(
                    new[] { "HMBDebug", this.channel.Name, "Cannot join channel (+b) - you are banned" });

            this.clientMock.Raise(
                x => x.ReceivedMessage += null,
                new MessageReceivedEventArgs(message.Object, this.clientMock.Object));

            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Exactly(2));
            Assert.That(this.channel.Enabled, Is.False);
        }

        [Test]
        public void TestBadKeyJoin()
        {
            var command = new JoinCommand(
                "##test",
                new IrcUser { Account = "test", Hostname = "test", Nickname = "test", Username = "test" },
                new[] { "##test" },
                this.Logger.Object,
                this.DatabaseSession.Object,
                this.commandServiceHelperMock.Object);

            var result = command.Run();
            Assert.That(result.IsNullOrEmpty());
            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Once());
            Assert.That(this.channel.Enabled, Is.True);

            var message = new Mock<IMessage>();
            message.SetupGet(x => x.Command).Returns("475");
            message.SetupGet(x => x.Parameters)
                .Returns(
                    new[] { "HMBDebug", this.channel.Name, "Cannot join channel (+k) - bad key" });

            this.clientMock.Raise(
                x => x.ReceivedMessage += null,
                new MessageReceivedEventArgs(message.Object, this.clientMock.Object));

            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Exactly(2));
            Assert.That(this.channel.Enabled, Is.False);
        }

        [Test]
        public void TestThrottledJoin()
        {
            var command = new JoinCommand(
                "##test",
                new IrcUser { Account = "test", Hostname = "test", Nickname = "test", Username = "test" },
                new[] { "##test" },
                this.Logger.Object,
                this.DatabaseSession.Object,
                this.commandServiceHelperMock.Object);

            var result = command.Run();
            Assert.That(result.IsNullOrEmpty());
            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Once());
            Assert.That(this.channel.Enabled, Is.True);

            var message = new Mock<IMessage>();
            message.SetupGet(x => x.Command).Returns("480");
            message.SetupGet(x => x.Parameters)
                .Returns(
                    new[] { "HMBDebug", this.channel.Name, "Cannot join channel (+j) - throttle exceeded, try again later" });

            this.clientMock.Raise(
                x => x.ReceivedMessage += null,
                new MessageReceivedEventArgs(message.Object, this.clientMock.Object));

            this.DatabaseSession.Verify(x => x.Save(this.channel), Times.Exactly(1));
            Assert.That(this.channel.Enabled, Is.True);
        }
    }
}