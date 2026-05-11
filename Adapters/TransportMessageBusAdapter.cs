using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dreamine.Communication.Abstractions.Enums;
using Dreamine.Communication.Abstractions.Interfaces;
using Dreamine.Communication.Abstractions.Models;

namespace Dreamine.Communication.Core.Adapters;

/// <summary>
/// \brief IMessageTransport를 IMessageBus 형태로 사용하는 어댑터입니다.
/// </summary>
public sealed class TransportMessageBusAdapter : IMessageBus
{
    private readonly IMessageTransport _transport;
    private readonly ConcurrentDictionary<string, List<Func<MessageEnvelope, CancellationToken, Task>>> _handlers = new();

    /// <summary>
    /// \brief TransportMessageBusAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="transport">내부에서 사용할 메시지 전송 계층입니다.</param>
    public TransportMessageBusAdapter(IMessageTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _transport.MessageReceived += OnMessageReceived;
    }

    /// <summary>
    /// \brief 메시지 버스의 현재 연결 상태를 가져옵니다.
    /// </summary>
    public ConnectionState State => _transport.State;

    /// <summary>
    /// \brief 메시지 버스 종류를 가져옵니다.
    /// </summary>
    public TransportKind Kind => _transport.Kind;

    /// <summary>
    /// \brief 내부 Transport에 연결합니다.
    /// </summary>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        return _transport.ConnectAsync(cancellationToken);
    }

    /// <summary>
    /// \brief 내부 Transport를 통해 메시지를 발행합니다.
    /// </summary>
    /// <param name="message">발행할 메시지입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public Task PublishAsync(
        MessageEnvelope message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return _transport.SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// \brief 지정한 라우트의 메시지를 구독합니다.
    /// </summary>
    /// <param name="route">구독할 라우트입니다.</param>
    /// <param name="handler">메시지 처리기입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public Task SubscribeAsync(
        string route,
        Func<MessageEnvelope, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentNullException.ThrowIfNull(handler);
        cancellationToken.ThrowIfCancellationRequested();

        var handlers = _handlers.GetOrAdd(
            route,
            _ => new List<Func<MessageEnvelope, CancellationToken, Task>>());

        lock (handlers)
        {
            handlers.Add(handler);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// \brief 내부 Transport 연결을 해제합니다.
    /// </summary>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return _transport.DisconnectAsync(cancellationToken);
    }

    /// <summary>
    /// \brief 어댑터와 내부 Transport 리소스를 비동기로 해제합니다.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _transport.MessageReceived -= OnMessageReceived;
        await _transport.DisposeAsync().ConfigureAwait(false);
    }

    private async void OnMessageReceived(object? sender, MessageEnvelope message)
    {
        if (!_handlers.TryGetValue(message.Route, out var handlers))
        {
            return;
        }

        List<Func<MessageEnvelope, CancellationToken, Task>> snapshot;

        lock (handlers)
        {
            snapshot = new List<Func<MessageEnvelope, CancellationToken, Task>>(handlers);
        }

        foreach (var handler in snapshot)
        {
            await handler(message, CancellationToken.None).ConfigureAwait(false);
        }
    }
}