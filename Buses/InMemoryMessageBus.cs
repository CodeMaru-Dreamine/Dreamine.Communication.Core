using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dreamine.Communication.Abstractions.Enums;
using Dreamine.Communication.Abstractions.Interfaces;
using Dreamine.Communication.Abstractions.Models;

namespace Dreamine.Communication.Core.Buses;

/// <summary>
/// \brief 메모리 내부에서 메시지를 발행하고 구독하는 메시지 버스입니다.
/// </summary>
public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly ConcurrentDictionary<string, List<Func<MessageEnvelope, CancellationToken, Task>>> _handlers = new();

    /// <summary>
    /// \brief 메시지 버스의 현재 연결 상태를 가져옵니다.
    /// </summary>
    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    /// <summary>
    /// \brief 메시지 버스 종류를 가져옵니다.
    /// </summary>
    public TransportKind Kind => TransportKind.InMemory;

    /// <summary>
    /// \brief 메시지 버스에 연결합니다.
    /// </summary>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        State = ConnectionState.Connected;
        return Task.CompletedTask;
    }

    /// <summary>
    /// \brief 메시지를 발행합니다.
    /// </summary>
    /// <param name="message">발행할 메시지입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public async Task PublishAsync(
        MessageEnvelope message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (State != ConnectionState.Connected)
        {
            throw new InvalidOperationException("The message bus is not connected.");
        }

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
            cancellationToken.ThrowIfCancellationRequested();
            await handler(message, cancellationToken).ConfigureAwait(false);
        }
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
    /// \brief 메시지 버스 연결을 해제합니다.
    /// </summary>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        State = ConnectionState.Disconnected;
        return Task.CompletedTask;
    }

    /// <summary>
    /// \brief 메시지 버스 리소스를 비동기로 해제합니다.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        _handlers.Clear();
        State = ConnectionState.Disconnected;
        return ValueTask.CompletedTask;
    }
}