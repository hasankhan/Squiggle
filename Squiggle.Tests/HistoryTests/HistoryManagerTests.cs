using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Squiggle.History;
using Squiggle.History.DAL;
using Squiggle.History.DAL.Entities;
using Xunit;

namespace Squiggle.Tests.HistoryTests
{
    public class HistoryManagerTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly HistoryManager _manager;

        public HistoryManagerTests()
        {
            _dbPath = Path.Combine(
                AppContext.BaseDirectory,
                $"test_history_{Guid.NewGuid():N}.db");
            _connectionString = $"Data Source={_dbPath}";
            _manager = new HistoryManager(_connectionString);
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(_dbPath))
                    File.Delete(_dbPath);
            }
            catch { }
        }

        private Session CreateSession(string? id = null)
        {
            return new Session
            {
                Id = id ?? Guid.NewGuid().ToString(),
                ContactId = "contact1",
                ContactName = "Test User",
                Start = DateTime.UtcNow
            };
        }

        private IEnumerable<Participant> CreateParticipants(string sessionId)
        {
            return new[]
            {
                new Participant
                {
                    Id = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    ContactId = "contact1",
                    ContactName = "Test User"
                }
            };
        }

        [Fact]
        public void AddSession_GetSession_RoundTrip()
        {
            var session = CreateSession();
            _manager.AddSession(session, CreateParticipants(session.Id));

            var retrieved = _manager.GetSession(session.Id);

            retrieved.Should().NotBeNull();
            retrieved!.Id.Should().Be(session.Id);
            retrieved.ContactId.Should().Be("contact1");
            retrieved.ContactName.Should().Be("Test User");
        }

        [Fact]
        public void GetSession_ReturnsNull_WhenNotFound()
        {
            // Trigger DB creation by adding then querying a non-existent session
            var session = CreateSession();
            _manager.AddSession(session, CreateParticipants(session.Id));

            var result = _manager.GetSession("non-existent-id");

            result.Should().BeNull();
        }

        [Fact]
        public void AddSessionEvent_AppearsInSession()
        {
            var session = CreateSession();
            _manager.AddSession(session, CreateParticipants(session.Id));

            _manager.AddSessionEvent(
                session.Id,
                EventType.Message,
                "contact1",
                "Test User",
                new[] { "contact2" },
                "Assalamu Alaikum");

            var retrieved = _manager.GetSession(session.Id);
            retrieved.Should().NotBeNull();
            retrieved!.Events.Should().HaveCount(1);
            retrieved.Events.First().Data.Should().Be("Assalamu Alaikum");
            retrieved.Events.First().Type.Should().Be(EventType.Message);
        }

        [Fact]
        public void AddSessionEvent_WithJoinedType_AddsParticipant()
        {
            var session = CreateSession();
            _manager.AddSession(session, CreateParticipants(session.Id));

            _manager.AddSessionEvent(
                session.Id,
                EventType.Joined,
                "contact2",
                "New User",
                Array.Empty<string>(),
                "");

            var retrieved = _manager.GetSession(session.Id);
            retrieved.Should().NotBeNull();
            retrieved!.Participants.Should().Contain(p => p.ContactId == "contact2");
        }

        [Fact]
        public void AddStatusUpdate_GetStatusUpdates_RoundTrip()
        {
            // Trigger DB creation
            var session = CreateSession();
            _manager.AddSession(session, CreateParticipants(session.Id));

            _manager.AddStatusUpdate("contact1", "Test User", 1);

            var updates = _manager.GetStatusUpdates(new StatusCriteria()).ToList();
            updates.Should().NotBeEmpty();
            updates.First().ContactId.Should().Be("contact1");
            updates.First().StatusCode.Should().Be(1);
        }

        [Fact]
        public void ClearChatHistory_RemovesSessions()
        {
            var session = CreateSession();
            _manager.AddSession(session, CreateParticipants(session.Id));

            _manager.ClearChatHistory();

            var result = _manager.GetSession(session.Id);
            result.Should().BeNull();
        }

        [Fact]
        public void DeleteSessions_RemovesSpecificSessions()
        {
            var session1 = CreateSession();
            var session2 = CreateSession();
            _manager.AddSession(session1, CreateParticipants(session1.Id));
            _manager.AddSession(session2, CreateParticipants(session2.Id));

            _manager.DeleteSessions(new[] { session1.Id });

            _manager.GetSession(session1.Id).Should().BeNull();
            _manager.GetSession(session2.Id).Should().NotBeNull();
        }

        [Fact]
        public void GetSessions_FiltersByParticipant()
        {
            var session = CreateSession();
            _manager.AddSession(session, CreateParticipants(session.Id));

            var criteria = new SessionCriteria { Participant = "contact1" };
            var sessions = _manager.GetSessions(criteria).ToList();

            sessions.Should().ContainSingle();
            sessions.First().Id.Should().Be(session.Id);
        }

        [Fact]
        public void GetSessions_ReturnsEmpty_WhenNoMatch()
        {
            var session = CreateSession();
            _manager.AddSession(session, CreateParticipants(session.Id));

            var criteria = new SessionCriteria { Participant = "no-such-contact" };
            var sessions = _manager.GetSessions(criteria).ToList();

            sessions.Should().BeEmpty();
        }
    }
}
