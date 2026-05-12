using Dreamine.Communication.Abstractions.Interfaces;
using Dreamine.Communication.Abstractions.Models;
using Dreamine.Communication.Core.Serialization;

namespace Dreamine.Communication.Core.Protocols;

/// <summary>
/// \brief Dreamine 표준 MessageEnvelope JSON 프로토콜 어댑터입니다.
/// </summary>
/// <remarks>
/// 이 어댑터는 기존 Dreamine 내부 통신 방식인 MessageEnvelope JSON 직렬화 방식을 유지합니다.
/// </remarks>
public sealed class DreamineEnvelopeProtocolAdapter : IMessageProtocolAdapter
{
    private readonly IMessageSerializer _serializer;

    /// <summary>
    /// \brief DreamineEnvelopeProtocolAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public DreamineEnvelopeProtocolAdapter()
        : this(new JsonMessageSerializer())
    {
    }

    /// <summary>
    /// \brief DreamineEnvelopeProtocolAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="serializer">MessageEnvelope 직렬화기입니다.</param>
    public DreamineEnvelopeProtocolAdapter(IMessageSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// \brief 수신된 Dreamine MessageEnvelope JSON 데이터를 MessageEnvelope로 변환합니다.
    /// </summary>
    /// <param name="payload">수신된 MessageEnvelope JSON 데이터입니다.</param>
    /// <returns>Dreamine 내부 표준 메시지입니다.</returns>
    public MessageEnvelope Decode(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return _serializer.Deserialize(payload);
    }

    /// <summary>
    /// \brief MessageEnvelope를 Dreamine 표준 JSON 데이터로 변환합니다.
    /// </summary>
    /// <param name="message">송신할 MessageEnvelope입니다.</param>
    /// <returns>송신할 Dreamine 표준 JSON 데이터입니다.</returns>
    public byte[] Encode(MessageEnvelope message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return _serializer.Serialize(message);
    }
}