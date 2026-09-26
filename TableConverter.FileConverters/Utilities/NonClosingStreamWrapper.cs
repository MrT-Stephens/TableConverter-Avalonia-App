namespace TableConverter.FileConverters.Utilities;

/// <summary>
///     Wraps a stream so that disposing or closing the wrapper does not close the underlying stream.
/// </summary>
/// <remarks>
///     Some third party document writers (for example NPOI) close the stream they are handed once the
///     document has been written. The converters do not own that stream - the caller does - so they wrap
///     it to stop it being closed while it is still in use.
/// </remarks>
public sealed class NonClosingStreamWrapper(Stream inner) : Stream
{
    private readonly Stream _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;
    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override void Flush() => _inner.Flush();

    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

    public override void SetLength(long value) => _inner.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _inner.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _inner.WriteAsync(buffer, cancellationToken);
    }

    public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

    /// <summary>
    ///     Flushes the underlying stream but deliberately leaves it open.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Flush();
        }

        base.Dispose(disposing);
    }
}

