using System.Text;

namespace Dreamine.Communication.Core.Framing;

/// <summary>
/// \brief 구분자 기반 메시지 프레임 코덱입니다.
/// </summary>
public sealed class DelimiterMessageFrameCodec : IMessageFrameCodec
{
    private readonly byte[] _delimiter;
    private readonly int _maxFrameLength;

    /// <summary>
    /// \brief DelimiterMessageFrameCodec 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public DelimiterMessageFrameCodec()
        : this("\r\n", Encoding.UTF8, 1024 * 1024)
    {
    }

    /// <summary>
    /// \brief DelimiterMessageFrameCodec 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="delimiter">메시지 구분자입니다.</param>
    /// <param name="encoding">구분자 인코딩입니다.</param>
    /// <param name="maxFrameLength">최대 프레임 길이입니다.</param>
    public DelimiterMessageFrameCodec(
        string delimiter,
        Encoding encoding,
        int maxFrameLength)
    {
        ArgumentException.ThrowIfNullOrEmpty(delimiter);
        ArgumentNullException.ThrowIfNull(encoding);

        if (maxFrameLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFrameLength));
        }

        _delimiter = encoding.GetBytes(delimiter);
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

        await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(_delimiter, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// \brief 스트림에서 구분자까지 하나의 메시지 프레임을 읽습니다.
    /// </summary>
    /// <param name="stream">대상 스트림입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    /// <returns>읽은 메시지 데이터입니다. 연결이 종료되면 null입니다.</returns>
    public async Task<byte[]?> ReadFrameAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var buffer = new List<byte>();
        var temp = new byte[1];

        while (true)
        {
            var read = await stream.ReadAsync(
                    temp.AsMemory(0, 1),
                    cancellationToken)
                .ConfigureAwait(false);

            if (read == 0)
            {
                return buffer.Count == 0
                    ? null
                    : buffer.ToArray();
            }

            buffer.Add(temp[0]);

            if (buffer.Count > _maxFrameLength)
            {
                throw new InvalidDataException(
                    $"Frame length exceeded. MaxFrameLength={_maxFrameLength}");
            }

            if (EndsWithDelimiter(buffer))
            {
                buffer.RemoveRange(
                    buffer.Count - _delimiter.Length,
                    _delimiter.Length);

                return buffer.ToArray();
            }
        }
    }

    private bool EndsWithDelimiter(List<byte> buffer)
    {
        if (buffer.Count < _delimiter.Length)
        {
            return false;
        }

        var start = buffer.Count - _delimiter.Length;

        for (var index = 0; index < _delimiter.Length; index++)
        {
            if (buffer[start + index] != _delimiter[index])
            {
                return false;
            }
        }

        return true;
    }
}