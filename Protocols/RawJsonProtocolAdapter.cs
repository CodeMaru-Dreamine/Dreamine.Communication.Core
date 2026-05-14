using System.Text;
using System.Text.Json;
using Dreamine.Communication.Abstractions.Interfaces;
using Dreamine.Communication.Abstractions.Models;

namespace Dreamine.Communication.Core.Protocols;

/// <summary>
/// \brief 스키마가 고정되지 않은 외부 JSON 데이터를 MessageEnvelope로 감싸는 프로토콜 어댑터입니다.
/// </summary>
public sealed class RawJsonProtocolAdapter : IMessageProtocolAdapter
{
    private readonly Encoding _encoding;
    private readonly string _defaultRoute;
    private readonly string _defaultName;

    /// <summary>
    /// \brief RawJsonProtocolAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public RawJsonProtocolAdapter()
        : this(new RawJsonProtocolOptions())
    {
    }

    /// <summary>
    /// \brief RawJsonProtocolAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="defaultRoute">라우트 추출에 실패했을 때 사용할 기본 Route입니다.</param>
    /// <param name="defaultName">이름 추출에 실패했을 때 사용할 기본 Name입니다.</param>
    public RawJsonProtocolAdapter(string defaultRoute, string defaultName)
        : this(new RawJsonProtocolOptions
        {
            DefaultRoute = defaultRoute,
            DefaultName = defaultName
        })
    {
    }

    /// <summary>
    /// \brief RawJsonProtocolAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="encoding">외부 Raw JSON 송수신 문자열 인코딩입니다.</param>
    /// <param name="defaultRoute">라우트 추출에 실패했을 때 사용할 기본 Route입니다.</param>
    /// <param name="defaultName">이름 추출에 실패했을 때 사용할 기본 Name입니다.</param>
    public RawJsonProtocolAdapter(
        Encoding encoding,
        string defaultRoute,
        string defaultName)
        : this(new RawJsonProtocolOptions
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding)),
            DefaultRoute = defaultRoute,
            DefaultName = defaultName
        })
    {
    }

    /// <summary>
    /// \brief RawJsonProtocolAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="options">Raw JSON 프로토콜 어댑터 설정입니다.</param>
    public RawJsonProtocolAdapter(RawJsonProtocolOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.DefaultRoute);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.DefaultName);

        _encoding = options.Encoding ?? throw new ArgumentNullException(nameof(options.Encoding));
        _defaultRoute = options.DefaultRoute;
        _defaultName = options.DefaultName;
        NormalizePayloadToUtf8 = options.NormalizePayloadToUtf8;
    }

    /// <summary>
    /// \brief Decode된 JSON Payload를 Dreamine 내부 표준 UTF-8 Payload로 정규화할지 여부입니다.
    /// </summary>
    public bool NormalizePayloadToUtf8 { get; }

    /// <summary>
    /// \brief 원시 JSON 데이터를 MessageEnvelope로 변환합니다.
    /// </summary>
    /// <param name="payload">수신된 JSON 데이터입니다.</param>
    /// <returns>Dreamine 내부 표준 메시지입니다.</returns>
    public MessageEnvelope Decode(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var json = _encoding.GetString(payload);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var route = TryGetString(root, "route")
                    ?? TryGetString(root, "Route")
                    ?? TryGetString(root, "routingKey")
                    ?? TryGetString(root, "topic")
                    ?? TryGetString(root, "type")
                    ?? TryGetString(root, "cmd")
                    ?? _defaultRoute;

        var name = TryGetString(root, "name")
                   ?? TryGetString(root, "Name")
                   ?? TryGetString(root, "type")
                   ?? TryGetString(root, "cmd")
                   ?? _defaultName;

        return new MessageEnvelope
        {
            Name = name,
            Route = route,
            Payload = NormalizePayloadToUtf8
                ? Encoding.UTF8.GetBytes(json)
                : _encoding.GetBytes(json),
            Headers = new Dictionary<string, string>
            {
                ["ContentType"] = "application/json",
                ["Protocol"] = "RawJson",
                ["ExternalEncoding"] = _encoding.WebName
            }
        };
    }

    /// <summary>
    /// \brief MessageEnvelope를 외부 JSON 송신 데이터로 변환합니다.
    /// </summary>
    /// <param name="message">송신할 MessageEnvelope입니다.</param>
    /// <returns>송신할 JSON 데이터입니다.</returns>
    public byte[] Encode(MessageEnvelope message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Payload.Length > 0)
        {
            var json = Encoding.UTF8.GetString(message.Payload);
            return _encoding.GetBytes(json);
        }

        var generatedJson = JsonSerializer.Serialize(new
        {
            name = message.Name,
            route = message.Route
        });

        return _encoding.GetBytes(generatedJson);
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();
    }
}
