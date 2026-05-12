using System.Buffers.Binary;

namespace Dreamine.Communication.Core.Framing;

/// <summary>
/// \brief 4바이트 길이 접두사를 사용하는 메시지 프레임 코덱입니다.
/// </summary>
public sealed class LengthPrefixedMessageFrameCodec : IMessageFrameCodec
{
    private const int HeaderSize = 4;

    private readonly int _maxFrameLength;

    /// <summary>
    /// \brief LengthPrefixedMessageFrameCodec 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public LengthPrefixedMessageFrameCodec()
        : this(1024 * 1024)
    {
    }

    /// <summary>
    /// \brief LengthPrefixedMessageFrameCodec 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="maxFrameLength">최대 프레임 길이입니다.</param>
    public LengthPrefixedMessageFrameCodec(int maxFrameLength)
    {
        if (maxFrameLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFrameLength));
        }

        _maxFrameLength = maxFrameLength;
    }

    /// <summary>
    /// \brief 하나의 메시지 프레임을 스트림에 기록합니다.
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

        if (payload.Length > _maxFrameLength)
        {
            throw new InvalidDataException(
                $"Frame length exceeded. Length={payload.Length}, MaxFrameLength={_maxFrameLength}");
        }

        var header = new byte[HeaderSize];
        BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);

        await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// \brief 스트림에서 하나의 메시지 프레임을 읽습니다.
    /// </summary>
    /// <param name="stream">대상 스트림입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    /// <returns>읽은 메시지 데이터입니다. 연결이 종료되면 null입니다.</returns>
    public async Task<byte[]?> ReadFrameAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var header = await ReadExactAsync(stream, HeaderSize, cancellationToken)
            .ConfigureAwait(false);

        if (header is null)
        {
            return null;
        }

        var length = BinaryPrimitives.ReadInt32BigEndian(header);

        if (length < 0 || length > _maxFrameLength)
        {
            throw new InvalidDataException(
                $"Invalid frame length. Length={length}, MaxFrameLength={_maxFrameLength}");
        }

        if (length == 0)
        {
            return Array.Empty<byte>();
        }

        return await ReadExactAsync(stream, length, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<byte[]?> ReadExactAsync(
        Stream stream,
        int length,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        var offset = 0;

        while (offset < length)
        {
            var read = await stream.ReadAsync(
                    buffer.AsMemory(offset, length - offset),
                    cancellationToken)
                .ConfigureAwait(false);

            if (read == 0)
            {
                return null;
            }

            offset += read;
        }

        return buffer;
    }
}