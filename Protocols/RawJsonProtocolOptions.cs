using System.Text;

namespace Dreamine.Communication.Core.Protocols;

/// <summary>
/// \brief Raw JSON 프로토콜 어댑터 설정입니다.
/// </summary>
public sealed class RawJsonProtocolOptions
{
    /// <summary>
    /// \brief 외부 Raw JSON 송수신에 사용할 문자열 인코딩입니다.
    /// </summary>
    public Encoding Encoding { get; init; } = PlainTextProtocolOptions.CreateEncoding(PlainTextProtocolOptions.Utf8EncodingName);

    /// <summary>
    /// \brief 라우트 추출에 실패했을 때 사용할 기본 Route입니다.
    /// </summary>
    public string DefaultRoute { get; init; } = "external.json";

    /// <summary>
    /// \brief 이름 추출에 실패했을 때 사용할 기본 Name입니다.
    /// </summary>
    public string DefaultName { get; init; } = "External.Json";

    /// <summary>
    /// \brief Decode된 JSON Payload를 Dreamine 내부 표준 UTF-8 Payload로 정규화할지 여부입니다.
    /// </summary>
    public bool NormalizePayloadToUtf8 { get; init; } = true;
}
