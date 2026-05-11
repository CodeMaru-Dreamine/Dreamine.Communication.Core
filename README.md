# Dreamine.Communication.Core

`Dreamine.Communication.Core` is part of the Dreamine Communication package family.

This package provides the common runtime layer used by concrete communication adapters. It does not implement TCP, Serial, RabbitMQ, or WPF directly.

[➡️ 한국어 문서 보기](./README_KO.md)

## Description

Core message bus, routing, serialization, framing, and adapter utilities for Dreamine Communication.

## Features

- In-memory message bus
- Message routing foundation
- MessageEnvelope JSON serialization
- Shared message framing
- Transport-to-message-bus adapter

## Design Principles

- Keep concrete transport implementations isolated from upper layers.
- Depend on `Dreamine.Communication.Abstractions` contracts.
- Keep package responsibilities small and explicit.
- Preserve one-way dependency flow.
- Allow future adapters to be added without changing application logic.

## Package Role

```text
Dreamine.Communication.Abstractions
    ↑
Dreamine.Communication.Core
    ↑
Sockets / Serial / RabbitMQ / WPF
```

## Dependencies

- `Dreamine.Communication.Abstractions`

## Target Framework

```text
net8.0
```

## Related Packages

- `Dreamine.Communication.Abstractions`
- `Dreamine.Communication.Core`
- `Dreamine.Communication.Sockets`
- `Dreamine.Communication.Serial`
- `Dreamine.Communication.RabbitMQ`
- `Dreamine.Communication.FullKit`
- `Dreamine.Communication.Wpf`

## License

This project is licensed under the MIT License.
