using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dreamine.Communication.Core.Framing;

/// <summary>
/// \brief 4바이트 길이 접두사를 사용하는 메시지 프레임 코덱입니다.
/// </summary>
public sealed class LengthPrefixedMessageFrameCodec : IMessageFrameCodec
{
    private const int HeaderSize = 4;

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

        if (length < 0)
        {
            throw new InvalidDataException("Invalid frame length.");
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
                cancellationToken).ConfigureAwait(false);

            if (read == 0)
            {
                return null;
            }

            offset += read;
        }

        return buffer;
    }
}