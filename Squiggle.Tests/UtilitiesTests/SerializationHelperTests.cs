using System;
using System.IO;
using FluentAssertions;
using ProtoBuf;
using Squiggle.Utilities.Serialization;
using Xunit;

namespace Squiggle.Tests.UtilitiesTests
{
    public class SerializationHelperTests
    {
        [ProtoContract]
        public class TestPayload
        {
            [ProtoMember(1)]
            public int Id { get; set; }

            [ProtoMember(2)]
            public string Name { get; set; } = null!;

            [ProtoMember(3)]
            public double Value { get; set; }
        }

        [Fact]
        public void Serialize_Deserialize_RoundTrip_PreservesData()
        {
            var original = new TestPayload { Id = 42, Name = "test", Value = 3.14 };

            byte[] bytes = SerializationHelper.Serialize(original);

            bytes.Should().NotBeNullOrEmpty();

            // Deserialize using ProtoBuf directly since the helper's Deserialize is private
            var deserialized = ProtoBuf.Serializer.Deserialize<TestPayload>(new MemoryStream(bytes));

            deserialized.Id.Should().Be(42);
            deserialized.Name.Should().Be("test");
            deserialized.Value.Should().BeApproximately(3.14, 0.001);
        }

        [Fact]
        public void Serialize_EmptyObject_ProducesValidBytes()
        {
            var empty = new TestPayload();

            byte[] bytes = SerializationHelper.Serialize(empty);

            // Protobuf serialization of default values may produce empty byte array
            bytes.Should().NotBeNull();
        }

        [Fact]
        public void Deserialize_WithCallback_InvokesCallback_OnValidData()
        {
            var original = new TestPayload { Id = 7, Name = "callback", Value = 1.0 };
            byte[] bytes = SerializationHelper.Serialize(original);
            TestPayload? received = null;

            SerializationHelper.Deserialize<TestPayload>(bytes, obj => received = obj, "TestPayload");

            received.Should().NotBeNull();
            received!.Id.Should().Be(7);
            received.Name.Should().Be("callback");
        }

        [Fact]
        public void Deserialize_WithCallback_DoesNotInvokeCallback_OnInvalidData()
        {
            byte[] garbage = { 0xFF, 0xFE, 0xFD, 0xFC, 0xFB };
            TestPayload? received = null;

            // Should eat the exception and not call the callback
            SerializationHelper.Deserialize<TestPayload>(garbage, obj => received = obj, "TestPayload");

            received.Should().BeNull();
        }

        [Fact]
        public void Serialize_PreservesNullableStrings()
        {
            var payload = new TestPayload { Id = 1, Name = null!, Value = 0 };

            byte[] bytes = SerializationHelper.Serialize(payload);
            var deserialized = ProtoBuf.Serializer.Deserialize<TestPayload>(new MemoryStream(bytes));

            deserialized.Name.Should().BeNull();
        }
    }
}
