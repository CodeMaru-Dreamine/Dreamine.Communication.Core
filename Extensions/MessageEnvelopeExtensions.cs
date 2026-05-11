using System.Text;
using Dreamine.Communication.Abstractions.Models;

namespace Dreamine.Communication.Core.Extensions;

/// <summary>
/// \brief MessageEnvelope 편의 기능을 제공합니다.
/// </summary>
public static class MessageEnvelopeExtensions
{
    /// <summary>
    /// \brief Payload를 UTF-8 문자열로 변환합니다.
    /// </summary>
    /// <param name="message">대상 메시지입니다.</param>
    /// <returns>Payload 문자열입니다.</returns>
    public static string GetPayloadAsString(this MessageEnvelope message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return Encoding.UTF8.GetString(message.Payload);
    }

    /// <summary>
    /// \brief 문자열 Payload를 가진 MessageEnvelope를 생성합니다.
    /// </summary>
    /// <param name="route">메시지 라우트입니다.</param>
    /// <param name="name">메시지 이름입니다.</param>
    /// <param name="payload">문자열 Payload입니다.</param>
    /// <returns>생성된 메시지입니다.</returns>
    public static MessageEnvelope FromStringPayload(
        string route,
        string name,
        string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new MessageEnvelope
        {
            Route = route,
            Name = name,
            Payload = Encoding.UTF8.GetBytes(payload ?? string.Empty)
        };
    }
}