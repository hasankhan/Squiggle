using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Squiggle.Utilities;
using Xunit;

namespace Squiggle.Tests.UtilitiesTests
{
    public class ExceptionMonsterTests : IDisposable
    {
        public ExceptionMonsterTests()
        {
            ExceptionMonster.Logger = null;
        }

        public void Dispose()
        {
            ExceptionMonster.Logger = null;
        }

        [Fact]
        public void EatTheException_ReturnsTrue_WhenActionSucceeds()
        {
            bool result = ExceptionMonster.EatTheException(() => { }, "test action");

            result.Should().BeTrue();
        }

        [Fact]
        public void EatTheException_ReturnsFalse_WhenActionThrows()
        {
            bool result = ExceptionMonster.EatTheException(
                () => throw new InvalidOperationException("boom"),
                "test action");

            result.Should().BeFalse();
        }

        [Fact]
        public void EatTheException_OutputsException_WhenActionThrows()
        {
            var thrownException = new InvalidOperationException("test error");

            bool result = ExceptionMonster.EatTheException(
                () => throw thrownException,
                "test action",
                out Exception? caughtException);

            result.Should().BeFalse();
            caughtException.Should().BeSameAs(thrownException);
        }

        [Fact]
        public void EatTheException_LogsError_WhenLoggerIsSet()
        {
            var loggedMessages = new System.Collections.Generic.List<string>();
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
            logger.When(x => x.Log(
                    Arg.Any<LogLevel>(),
                    Arg.Any<EventId>(),
                    Arg.Any<object>(),
                    Arg.Any<Exception?>(),
                    Arg.Any<Func<object, Exception?, string>>()))
                .Do(ci => loggedMessages.Add(ci.ArgAt<object>(2)?.ToString() ?? ""));

            ExceptionMonster.Logger = logger;

            ExceptionMonster.EatTheException(
                () => throw new InvalidOperationException("logged error"),
                "failing action");

            loggedMessages.Should().NotBeEmpty();
        }

        [Fact]
        public void EatTheException_Generic_ReturnsValue_WhenActionSucceeds()
        {
            int result = ExceptionMonster.EatTheException(() => 42, "compute");

            result.Should().Be(42);
        }

        [Fact]
        public void EatTheException_Generic_ReturnsDefault_WhenActionThrows()
        {
            int result = ExceptionMonster.EatTheException<int>(
                () => throw new Exception("fail"),
                "compute");

            result.Should().Be(0);
        }

        [Fact]
        public void EatTheException_Generic_SetsSuccessFlag()
        {
            var value = ExceptionMonster.EatTheException(
                () => "hello",
                "test",
                out bool success,
                out Exception? ex);

            success.Should().BeTrue();
            ex.Should().BeNull();
            value.Should().Be("hello");
        }
    }
}
