# Squiggle — LAN Messenger

Squiggle is a free, open-source **peer-to-peer LAN instant messenger**. There is no central server — peers discover each other via multicast and communicate directly on the local network.

## Features

- Instant messaging over LAN
- File transfer
- Voice chat
- Screen capture & sharing
- Chat history
- Message translation
- Cross-subnet bridging

## Technology Stack

- **.NET 9** (targeting `net9.0-windows`)
- **Avalonia UI** — cross-platform UI framework (replacing legacy WPF)
- **protobuf-net** — wire protocol serialization
- **NAudio** — voice chat audio
- **Entity Framework 6 + SQLite** — chat history persistence

## Build

Squiggle uses the `dotnet` CLI to build:

```bash
dotnet build Squiggle.sln
```

## Run

```bash
dotnet run --project Squiggle.UI.Avalonia
```

## Architecture

```
Squiggle.UI.Avalonia  (Avalonia app — windows, views, plugin loader)
    ↓
Squiggle.Client       (Facade — ChatClient exposes buddy list, login, chat events)
    ↓
Squiggle.Core         (Networking — presence discovery, chat transport, message serialization)
    ↓
Squiggle.Infrastructure  (Shared abstractions — async pipes, serialization helpers)
Squiggle.Utilities       (Cross-cutting helpers used by all layers)
```

### Feature modules

| Project | Purpose |
|---|---|
| `Squiggle.FileTransfer` | P2P file transfer |
| `Squiggle.VoiceChat` | Voice chat via NAudio + Speex |
| `Squiggle.Screenshot` | Screen capture & send |
| `Squiggle.Translate` | Message translation |
| `Squiggle.History` | Chat history persistence |
| `Squiggle.Multicast` | Multicast presence (standalone exe) |
| `Squiggle.Bridge` | Cross-subnet/WAN bridging (standalone exe) |

## License

See [LICENSE](LICENSE) for details.
