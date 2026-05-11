using System;
using System.Net;
using FluentAssertions;
using NSubstitute;
using Squiggle.Client;
using Squiggle.Core.Presence;
using Squiggle.Plugins;
using Xunit;

namespace Squiggle.Tests.RecordTests
{
    public class RecordEqualityTests
    {
        [Fact]
        public void SignInOptions_SameValues_AreEqual()
        {
            var a = new SignInOptions
            {
                Username = "ahmad",
                DisplayName = "Ahmad",
                GroupName = "Team",
                Domain = "local"
            };
            var b = new SignInOptions
            {
                Username = "ahmad",
                DisplayName = "Ahmad",
                GroupName = "Team",
                Domain = "local"
            };

            a.Should().Be(b);
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void SignInOptions_DifferentValues_AreNotEqual()
        {
            var a = new SignInOptions { DisplayName = "Ahmad" };
            var b = new SignInOptions { DisplayName = "Bilal" };

            a.Should().NotBe(b);
        }

        [Fact]
        public void SignInOptions_NullValues_AreEqual()
        {
            var a = new SignInOptions();
            var b = new SignInOptions();

            a.Should().Be(b);
        }

        [Fact]
        public void LoginOptions_SameValues_AreEqual()
        {
            var buddyProps = Substitute.For<IBuddyProperties>();
            var chatEp = new IPEndPoint(IPAddress.Parse("10.0.0.1"), 9000);
            var multicastEp = new IPEndPoint(IPAddress.Parse("224.0.0.1"), 9001);
            var multicastRecvEp = new IPEndPoint(IPAddress.Parse("224.0.0.1"), 9002);
            var presenceEp = new IPEndPoint(IPAddress.Parse("10.0.0.1"), 9003);

            var a = new LoginOptions
            {
                ChatEndPoint = chatEp,
                MulticastEndPoint = multicastEp,
                MulticastReceiveEndPoint = multicastRecvEp,
                PresenceServiceEndPoint = presenceEp,
                DisplayName = "Ahmad",
                UserProperties = buddyProps,
                KeepAliveTime = TimeSpan.FromSeconds(30)
            };
            var b = new LoginOptions
            {
                ChatEndPoint = chatEp,
                MulticastEndPoint = multicastEp,
                MulticastReceiveEndPoint = multicastRecvEp,
                PresenceServiceEndPoint = presenceEp,
                DisplayName = "Ahmad",
                UserProperties = buddyProps,
                KeepAliveTime = TimeSpan.FromSeconds(30)
            };

            a.Should().Be(b);
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void LoginOptions_DifferentDisplayName_AreNotEqual()
        {
            var buddyProps = Substitute.For<IBuddyProperties>();
            var chatEp = new IPEndPoint(IPAddress.Parse("10.0.0.1"), 9000);
            var multicastEp = new IPEndPoint(IPAddress.Parse("224.0.0.1"), 9001);
            var multicastRecvEp = new IPEndPoint(IPAddress.Parse("224.0.0.1"), 9002);
            var presenceEp = new IPEndPoint(IPAddress.Parse("10.0.0.1"), 9003);

            var a = new LoginOptions
            {
                ChatEndPoint = chatEp,
                MulticastEndPoint = multicastEp,
                MulticastReceiveEndPoint = multicastRecvEp,
                PresenceServiceEndPoint = presenceEp,
                DisplayName = "Ahmad",
                UserProperties = buddyProps
            };
            var b = new LoginOptions
            {
                ChatEndPoint = chatEp,
                MulticastEndPoint = multicastEp,
                MulticastReceiveEndPoint = multicastRecvEp,
                PresenceServiceEndPoint = presenceEp,
                DisplayName = "Bilal",
                UserProperties = buddyProps
            };

            a.Should().NotBe(b);
        }

        [Fact]
        public void LoginOptions_DifferentEndPoints_AreNotEqual()
        {
            var buddyProps = Substitute.For<IBuddyProperties>();
            var multicastEp = new IPEndPoint(IPAddress.Parse("224.0.0.1"), 9001);
            var multicastRecvEp = new IPEndPoint(IPAddress.Parse("224.0.0.1"), 9002);
            var presenceEp = new IPEndPoint(IPAddress.Parse("10.0.0.1"), 9003);

            var a = new LoginOptions
            {
                ChatEndPoint = new IPEndPoint(IPAddress.Parse("10.0.0.1"), 9000),
                MulticastEndPoint = multicastEp,
                MulticastReceiveEndPoint = multicastRecvEp,
                PresenceServiceEndPoint = presenceEp,
                DisplayName = "Ahmad",
                UserProperties = buddyProps
            };
            var b = new LoginOptions
            {
                ChatEndPoint = new IPEndPoint(IPAddress.Parse("10.0.0.2"), 9000),
                MulticastEndPoint = multicastEp,
                MulticastReceiveEndPoint = multicastRecvEp,
                PresenceServiceEndPoint = presenceEp,
                DisplayName = "Ahmad",
                UserProperties = buddyProps
            };

            a.Should().NotBe(b);
        }
    }
}
