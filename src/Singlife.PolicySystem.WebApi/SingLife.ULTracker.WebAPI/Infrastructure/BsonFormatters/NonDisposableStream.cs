using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.Infrastructure.BsonFormatters
{
    /// <summary>
    /// Stream that delegates to an inner stream.
    /// This Stream is present so that the inner stream is not closed
    /// even when Close() or Dispose() is called.
    /// </summary>
    /// <remarks>
    /// Source code from
    /// https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/Infrastructure/NonDisposableStream.cs
    /// </remarks>
    internal class NonDisposableStream : Stream
    {
        private readonly Stream innerStream;

        /// <summary>
        /// Initializes a new <see cref="NonDisposableStream"/>.
        /// </summary>
        /// <param name="innerStream">The stream which should not be closed or flushed.</param>
        public NonDisposableStream(Stream innerStream)
        {
            this.innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        }

        /// <summary>
        /// The inner stream this object delegates to.
        /// </summary>
        private Stream InnerStream => innerStream;

        /// <inheritdoc />
        public override bool CanRead => innerStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => innerStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => innerStream.CanWrite;

        /// <inheritdoc />
        public override long Length => innerStream.Length;
        /// <inheritdoc />
        public override long Position
        {
            get { return innerStream.Position; }
            set { innerStream.Position = value; }
        }

        /// <inheritdoc />
        public override int ReadTimeout
        {
            get { return innerStream.ReadTimeout; }
            set { innerStream.ReadTimeout = value; }
        }

        /// <inheritdoc />
        public override bool CanTimeout => innerStream.CanTimeout;

        /// <inheritdoc />
        public override int WriteTimeout
        {
            get { return innerStream.WriteTimeout; }
            set { innerStream.WriteTimeout = value; }
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override IAsyncResult BeginRead(
            byte[] buffer,
            int offset,
            int count,
            AsyncCallback callback,
            object state)
        {
            return innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc />
        public override int EndRead(IAsyncResult asyncResult)
        {
            return innerStream.EndRead(asyncResult);
        }

        /// <inheritdoc />
        public override IAsyncResult BeginWrite(
            byte[] buffer,
            int offset,
            int count,
            AsyncCallback callback,
            object state)
        {
            return innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdoc />
        public override void EndWrite(IAsyncResult asyncResult)
        {
            innerStream.EndWrite(asyncResult);
        }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public override int ReadByte()
        {
            return innerStream.ReadByte();
        }

        /// <inheritdoc />
        public override void Flush()
        {
            // Do nothing, we want to explicitly avoid flush because it turns on Chunked encoding.
        }

        /// <inheritdoc />
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        /// <inheritdoc />
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return innerStream.FlushAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }

        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override void WriteByte(byte value)
        {
            innerStream.WriteByte(value);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            // No-op. In CoreCLR this is equivalent to Close.
            // Given that we don't own the underlying stream, we never want to do anything interesting here.
        }
    }
}