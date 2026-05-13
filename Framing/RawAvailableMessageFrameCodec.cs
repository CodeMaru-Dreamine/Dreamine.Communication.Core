using System.Buffers;

namespace Dreamine.Communication.Core.Framing;

/// <summary>
/// \brief 수신 가능한 바이트를 즉시 하나의 메시지 프레임으로 처리하는 Raw 프레임 코덱입니다.
/// </summary>
/// <remarks>
/// TCP는 메시지 경계가 없는 스트림 기반 프로토콜입니다.
/// 이 코덱은 외부 테스트 툴 또는 단순 문자열 통신을 위한 호환 모드입니다.
/// 완전한 업무 프로토콜에는 LengthPrefixed 또는 Delimiter 방식을 권장합니다.
/// </remarks>
public sealed class RawAvailableMessageFrameCodec : IMessageFrameCodec
{
    private readonly int _bufferSize;

    /// <summary>
    /// \brief RawAvailableMessageFrameCodec 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public RawAvailableMessageFrameCodec()
        : this(8192)
    {
    }

    /// <summary>
    /// \brief RawAvailableMessageFrameCodec 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="bufferSize">1회 수신 버퍼 크기입니다.</param>
    public RawAvailableMessageFrameCodec(int bufferSize)
    {
        if (bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        _bufferSize = bufferSize;
    }

    /// <summary>
    /// \brief 메시지 프레임을 스트림에 기록합니다.
    /// </summary>
    /// <param name="stream">대상 스트림입니다.</param>
    /// <param name="payload">기록할 메시지 데이터입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public async Task WriteFrameAsync(
        Stream stream,
        byte[] payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(payload);

        await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// \brief 스트림에서 현재 수신 가능한 데이터를 하나의 메시지로 읽습니다.
    /// </summary>
    /// <param name="stream">대상 스트림입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    /// <returns>읽은 메시지 데이터입니다. 연결이 종료되면 null입니다.</returns>
    public async Task<byte[]?> ReadFrameAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);

        try
        {
            var read = await stream.ReadAsync(
                    buffer.AsMemory(0, _bufferSize),
                    cancellationToken)
                .ConfigureAwait(false);

            if (read == 0)
            {
                return null;
            }

            var payload = new byte[read];
            Buffer.BlockCopy(buffer, 0, payload, 0, read);

            return payload;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}