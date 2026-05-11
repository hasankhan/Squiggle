using System;
using System.Net;
using FluentAssertions;
using NSubstitute;
using Squiggle.Client;
using Squiggle.Core.Presence;
using Squiggle.Plugins.Authentication;
using Xunit;

namespace Squiggle.Tests.RecordTests
{
    public class RecordEqualityTests
    {
        [Fact]
        public void UserDetails_SameValues_AreEqual()
        {
            var a = new UserDetails
            {
                DisplayName = "Ahmad",
                DisplayMessage = "Online",
                GroupName = "Team",
                Email = "ahmad@example.com"
            };
            var b = new UserDetails
            {
                DisplayName = "Ahmad",
                DisplayMessage = "Online",
                GroupName = "Team",
                Email = "ahmad@example.com"
            };

            a.Should().Be(b);
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void UserDetails_DifferentValues_AreNotEqual()
        {
            var a = new UserDetails { DisplayName = "Ahmad" };
            var b = new UserDetails { DisplayName = "Bilal" };

            a.Should().NotBe(b);
        }

        [Fact]
        public void UserDetails_WithImage_EqualsByReference()
        {
            var image = new byte[] { 1, 2, 3 };
            var a = new UserDetails { Image = image };
            var b = new UserDetails { Image = image };

            // Record equality for byte[] uses reference equality
            a.Should().Be(b);
        }

        [Fact]
        public void UserDetails_WithDifferentImageArrays_AreNotEqual()
        {
            var a = new UserDetails { Image = new byte[] { 1, 2, 3 } };
            var b = new UserDetails { Image = new byte[] { 1, 2, 3 } };

            // byte[] uses reference equality in records
            a.Should().NotBe(b);
        }

        [Fact]
        public void UserDetails_NullValues_AreEqual()
        {
            var a = new UserDetails();
            var b = new UserDetails();

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
