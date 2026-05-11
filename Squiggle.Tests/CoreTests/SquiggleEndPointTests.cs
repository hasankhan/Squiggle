using System;
using System.IO;
using System.Net;
using FluentAssertions;
using ProtoBuf;
using Squiggle.Core;
using Xunit;

namespace Squiggle.Tests.CoreTests
{
    public class SquiggleEndPointTests
    {
        private static IPEndPoint MakeEndPoint(string ip = "192.168.1.1", int port = 9000)
            => new IPEndPoint(IPAddress.Parse(ip), port);

        [Fact]
        public void Constructor_SetsClientIdAndAddress()
        {
            var address = MakeEndPoint();
            var ep = new SquiggleEndPoint("client1", address);

            ep.ClientID.Should().Be("client1");
            ep.Address.Should().Be(address);
        }

        [Fact]
        public void Constructor_ThrowsOnNullOrEmptyId()
        {
            var address = MakeEndPoint();

            Action act = () => new SquiggleEndPoint("", address);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CopyConstructor_CopiesValues()
        {
            var original = new SquiggleEndPoint("client2", MakeEndPoint("10.0.0.1", 8080));
            var copy = new SquiggleEndPoint(original);

            copy.ClientID.Should().Be(original.ClientID);
            copy.Address.Should().Be(original.Address);
        }

        [Fact]
        public void Equals_ReturnsTrueForSameEndpoint()
        {
            var a = new SquiggleEndPoint("c1", MakeEndPoint());
            var b = new SquiggleEndPoint("c1", MakeEndPoint());

            a.Equals(b).Should().BeTrue();
        }

        [Fact]
        public void Equals_ReturnsFalseForDifferentClientId()
        {
            var a = new SquiggleEndPoint("c1", MakeEndPoint());
            var b = new SquiggleEndPoint("c2", MakeEndPoint());

            a.Equals(b).Should().BeFalse();
        }

        [Fact]
        public void Equals_ReturnsFalseForDifferentAddress()
        {
            var a = new SquiggleEndPoint("c1", MakeEndPoint("10.0.0.1", 9000));
            var b = new SquiggleEndPoint("c1", MakeEndPoint("10.0.0.2", 9000));

            a.Equals(b).Should().BeFalse();
        }

        [Fact]
        public void Equals_ReturnsFalseForNull()
        {
            var ep = new SquiggleEndPoint("c1", MakeEndPoint());

            ep.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_SameForEqualEndpoints()
        {
            var a = new SquiggleEndPoint("c1", MakeEndPoint());
            var b = new SquiggleEndPoint("c1", MakeEndPoint());

            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void GetHashCode_DiffersForDifferentEndpoints()
        {
            var a = new SquiggleEndPoint("c1", MakeEndPoint());
            var b = new SquiggleEndPoint("c2", MakeEndPoint("10.0.0.2", 8080));

            a.GetHashCode().Should().NotBe(b.GetHashCode());
        }

        [Fact]
        public void ToString_ContainsClientIdAndAddress()
        {
            var address = MakeEndPoint("192.168.1.100", 5555);
            var ep = new SquiggleEndPoint("myClient", address);

            string result = ep.ToString();

            result.Should().Contain("myClient");
            result.Should().Contain("@");
            result.Should().Be("myClient@192.168.1.100:5555");
        }

        [Fact]
        public void Protobuf_RoundTrip_PreservesAllFields()
        {
            var original = new SquiggleEndPoint("proto-client", MakeEndPoint("172.16.0.1", 7777));

            using var ms = new MemoryStream();
            Serializer.Serialize(ms, original);
            ms.Position = 0;
            var deserialized = Serializer.Deserialize<SquiggleEndPoint>(ms);

            deserialized.ClientID.Should().Be(original.ClientID);
            deserialized.Address.Should().Be(original.Address);
            deserialized.Should().Be(original);
        }
    }
}
