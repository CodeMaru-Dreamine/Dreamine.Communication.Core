using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dreamine.Communication.Core.Framing;

/// <summary>
/// \brief 바이트 스트림에서 메시지 프레임을 읽고 쓰는 계약입니다.
/// </summary>
public interface IMessageFrameCodec
{
    /// <summary>
    /// \brief 하나의 메시지 프레임을 스트림에 기록합니다.
    /// </summary>
    /// <param name="stream">대상 스트림입니다.</param>
    /// <param name="payload">기록할 메시지 데이터입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    Task WriteFrameAsync(
        Stream stream,
        byte[] payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// \brief 스트림에서 하나의 메시지 프레임을 읽습니다.
    /// </summary>
    /// <param name="stream">대상 스트림입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    /// <returns>읽은 메시지 데이터입니다. 연결이 종료되면 null입니다.</returns>
    Task<byte[]?> ReadFrameAsync(
        Stream stream,
        CancellationToken cancellationToken = default);
}