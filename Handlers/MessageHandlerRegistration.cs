using System;
using System.Threading;
using System.Threading.Tasks;
using Dreamine.Communication.Abstractions.Models;

namespace Dreamine.Communication.Core.Handlers;

/// <summary>
/// \brief 메시지 라우트와 처리기를 묶는 등록 정보입니다.
/// </summary>
public sealed class MessageHandlerRegistration
{
    /// <summary>
    /// \brief MessageHandlerRegistration 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="route">메시지 라우트입니다.</param>
    /// <param name="handler">메시지 처리기입니다.</param>
    public MessageHandlerRegistration(
        string route,
        Func<MessageEnvelope, CancellationToken, Task> handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentNullException.ThrowIfNull(handler);

        Route = route;
        Handler = handler;
    }

    /// <summary>
    /// \brief 메시지 라우트입니다.
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// \brief 메시지 처리기입니다.
    /// </summary>
    public Func<MessageEnvelope, CancellationToken, Task> Handler { get; }
}