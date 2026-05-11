using System;
using System.Text.Json;
using Dreamine.Communication.Abstractions.Interfaces;
using Dreamine.Communication.Abstractions.Models;

namespace Dreamine.Communication.Core.Serialization;

/// <summary>
/// \brief System.Text.Json 기반 MessageEnvelope 직렬화기입니다.
/// </summary>
public sealed class JsonMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// \brief JsonMessageSerializer 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public JsonMessageSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    /// <summary>
    /// \brief 지정한 JSON 옵션을 사용하여 JsonMessageSerializer 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="options">JSON 직렬화 옵션입니다.</param>
    public JsonMessageSerializer(JsonSerializerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// \brief 메시지를 바이트 배열로 직렬화합니다.
    /// </summary>
    /// <param name="message">직렬화할 메시지입니다.</param>
    /// <returns>직렬화된 바이트 배열입니다.</returns>
    public byte[] Serialize(MessageEnvelope message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.SerializeToUtf8Bytes(message, _options);
    }

    /// <summary>
    /// \brief 바이트 배열을 메시지로 역직렬화합니다.
    /// </summary>
    /// <param name="data">역직렬화할 바이트 배열입니다.</param>
    /// <returns>역직렬화된 메시지입니다.</returns>
    public MessageEnvelope Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var message = JsonSerializer.Deserialize<MessageEnvelope>(data, _options);

        if (message is null)
        {
            throw new InvalidOperationException("Failed to deserialize MessageEnvelope.");
        }

        return message;
    }
}