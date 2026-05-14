using System.Text;
using System.Threading;

namespace Dreamine.Communication.Core.Protocols;

/// <summary>
/// \brief PlainText 프로토콜 어댑터 설정입니다.
/// </summary>
public sealed class PlainTextProtocolOptions
{
    /// <summary>
    /// \brief UTF-8 인코딩 표시 이름입니다.
    /// </summary>
    public const string Utf8EncodingName = "UTF-8";

    /// <summary>
    /// \brief 한국어 Windows ANSI 코드 페이지 표시 이름입니다.
    /// </summary>
    public const string KoreanCodePage949EncodingName = "CP949";

    private static int _isCodePageProviderRegistered;

    /// <summary>
    /// \brief 외부 PlainText 송수신에 사용할 문자열 인코딩입니다.
    /// </summary>
    public Encoding Encoding { get; init; } = CreateEncoding(Utf8EncodingName);

    /// <summary>
    /// \brief Decode 시 생성할 MessageEnvelope의 Route입니다.
    /// </summary>
    public string Route { get; init; } = "external.text";

    /// <summary>
    /// \brief Decode 시 생성할 MessageEnvelope의 Name입니다.
    /// </summary>
    public string Name { get; init; } = "External.Text";

    /// <summary>
    /// \brief Decode된 문자열 Payload를 Dreamine 내부 표준 UTF-8 Payload로 정규화할지 여부입니다.
    /// </summary>
    public bool NormalizePayloadToUtf8 { get; init; } = true;

    /// <summary>
    /// \brief 인코딩 표시 이름으로 Encoding 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="encodingName">인코딩 표시 이름입니다. UTF-8 또는 CP949를 지원합니다.</param>
    /// <returns>생성된 Encoding 인스턴스입니다.</returns>
    public static Encoding CreateEncoding(string? encodingName)
    {
        if (string.IsNullOrWhiteSpace(encodingName))
        {
            return new UTF8Encoding(false, true);
        }

        if (string.Equals(encodingName, Utf8EncodingName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(encodingName, "UTF8", StringComparison.OrdinalIgnoreCase))
        {
            return new UTF8Encoding(false, true);
        }

        if (string.Equals(encodingName, KoreanCodePage949EncodingName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(encodingName, "949", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(encodingName, "ks_c_5601-1987", StringComparison.OrdinalIgnoreCase))
        {
            EnsureCodePageProviderRegistered();
            return Encoding.GetEncoding(949);
        }

        EnsureCodePageProviderRegistered();
        return Encoding.GetEncoding(encodingName);
    }

    private static void EnsureCodePageProviderRegistered()
    {
        if (Interlocked.Exchange(ref _isCodePageProviderRegistered, 1) == 1)
        {
            return;
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}
