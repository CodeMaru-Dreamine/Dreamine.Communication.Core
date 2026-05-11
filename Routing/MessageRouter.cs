using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dreamine.Communication.Abstractions.Interfaces;
using Dreamine.Communication.Abstractions.Models;

namespace Dreamine.Communication.Core.Routing;

/// <summary>
/// \brief 메시지 라우트 기준으로 처리기를 실행하는 기본 메시지 라우터입니다.
/// </summary>
public sealed class MessageRouter : IMessageRouter
{
    private readonly ConcurrentDictionary<string, List<Func<MessageEnvelope, CancellationToken, Task>>> _handlers = new();

    /// <summary>
    /// \brief 지정한 라우트에 메시지 처리기를 등록합니다.
    /// </summary>
    /// <param name="route">메시지 라우트입니다.</param>
    /// <param name="handler">메시지 처리기입니다.</param>
    public void Register(
        string route,
        Func<MessageEnvelope, CancellationToken, Task> handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentNullException.ThrowIfNull(handler);

        var handlers = _handlers.GetOrAdd(
            route,
            _ => new List<Func<MessageEnvelope, CancellationToken, Task>>());

        lock (handlers)
        {
            handlers.Add(handler);
        }
    }

    /// <summary>
    /// \brief 메시지를 라우트에 따라 처리합니다.
    /// </summary>
    /// <param name="message">라우팅할 메시지입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public async Task RouteAsync(
        MessageEnvelope message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

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
    /// \brief 등록된 모든 메시지 처리기를 제거합니다.
    /// </summary>
    public void Clear()
    {
        _handlers.Clear();
    }
}