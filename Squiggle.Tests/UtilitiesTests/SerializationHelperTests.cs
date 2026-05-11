using System;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Squiggle.Utilities.Serialization;
using Xunit;

namespace Squiggle.Tests.UtilitiesTests
{
    public class SerializationHelperTests
    {
        public class TestPayload
        {
            public int Id { get; set; }

            public string Name { get; set; } = null!;

            public double Value { get; set; }
        }

        [Fact]
        public void Serialize_Deserialize_RoundTrip_PreservesData()
        {
            var original = new TestPayload { Id = 42, Name = "test", Value = 3.14 };

            byte[] bytes = SerializationHelper.Serialize(original);

            bytes.Should().NotBeNullOrEmpty();

            var deserialized = JsonSerializer.Deserialize<TestPayload>(bytes);

            deserialized!.Id.Should().Be(42);
            deserialized.Name.Should().Be("test");
            deserialized.Value.Should().BeApproximately(3.14, 0.001);
        }

        [Fact]
        public void Serialize_EmptyObject_ProducesValidBytes()
        {
            var empty = new TestPayload();

            byte[] bytes = SerializationHelper.Serialize(empty);

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
    }
}
