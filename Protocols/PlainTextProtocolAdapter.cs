using System.Text;
using Dreamine.Communication.Abstractions.Interfaces;
using Dreamine.Communication.Abstractions.Models;

namespace Dreamine.Communication.Core.Protocols;

/// <summary>
/// \brief 일반 문자열 데이터를 MessageEnvelope로 변환하는 프로토콜 어댑터입니다.
/// </summary>
public sealed class PlainTextProtocolAdapter : IMessageProtocolAdapter
{
    private readonly Encoding _encoding;
    private readonly string _route;
    private readonly string _name;

    /// <summary>
    /// \brief PlainTextProtocolAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public PlainTextProtocolAdapter()
        : this(Encoding.UTF8, "external.text", "External.Text")
    {
    }

    /// <summary>
    /// \brief PlainTextProtocolAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="encoding">문자열 인코딩입니다.</param>
    /// <param name="route">생성할 MessageEnvelope의 Route입니다.</param>
    /// <param name="name">생성할 MessageEnvelope의 Name입니다.</param>
    public PlainTextProtocolAdapter(
        Encoding encoding,
        string route,
        string name)
    {
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _route = route;
        _name = name;
    }

    /// <summary>
    /// \brief 일반 문자열 바이트를 MessageEnvelope로 변환합니다.
    /// </summary>
    /// <param name="payload">수신된 문자열 바이트입니다.</param>
    /// <returns>Dreamine 내부 표준 메시지입니다.</returns>
    public MessageEnvelope Decode(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var text = _encoding.GetString(payload);

        return new MessageEnvelope
        {
            Name = _name,
            Route = _route,
            Payload = _encoding.GetBytes(text),
            Headers = new Dictionary<string, string>
            {
                ["ContentType"] = "text/plain",
                ["Protocol"] = "PlainText"
            }
        };
    }

    /// <summary>
    /// \brief MessageEnvelope의 Payload를 일반 문자열 송신 데이터로 변환합니다.
    /// </summary>
    /// <param name="message">송신할 MessageEnvelope입니다.</param>
    /// <returns>송신할 문자열 데이터입니다.</returns>
    public byte[] Encode(MessageEnvelope message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return message.Payload;
    }
}