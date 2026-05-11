# Dreamine.Communication.Core

`Dreamine.Communication.Core`는 Dreamine Communication 계열 패키지의 일부입니다.

이 패키지는 구체 통신 어댑터들이 공통으로 사용하는 런타임 계층을 제공합니다. TCP, Serial, RabbitMQ, WPF를 직접 구현하지 않습니다.

[➡️ English Version](./README.md)

## Description

Core message bus, routing, serialization, framing, and adapter utilities for Dreamine Communication.

## 주요 기능

- 메모리 기반 메시지 버스
- 메시지 라우팅 기반 구조
- MessageEnvelope JSON 직렬화
- 공통 메시지 프레임 처리
- Transport to MessageBus 어댑터

## 설계 원칙

- 구체 통신 구현체를 상위 레이어와 분리합니다.
- `Dreamine.Communication.Abstractions`의 계약에 의존합니다.
- 패키지 책임을 작고 명확하게 유지합니다.
- 단방향 의존성 흐름을 유지합니다.
- 향후 어댑터를 추가해도 애플리케이션 로직을 변경하지 않도록 합니다.

## 패키지 역할

```text
Dreamine.Communication.Abstractions
    ↑
Dreamine.Communication.Core
    ↑
Sockets / Serial / RabbitMQ / WPF
```

## 의존성

- `Dreamine.Communication.Abstractions`

## 대상 프레임워크

```text
net8.0
```

## 관련 패키지

- `Dreamine.Communication.Abstractions`
- `Dreamine.Communication.Core`
- `Dreamine.Communication.Sockets`
- `Dreamine.Communication.Serial`
- `Dreamine.Communication.RabbitMQ`
- `Dreamine.Communication.FullKit`
- `Dreamine.Communication.Wpf`

## 라이선스

이 프로젝트는 MIT 라이선스를 따릅니다.
