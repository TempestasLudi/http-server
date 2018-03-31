using System.IO;

namespace com.tempestasludi.c.http_source.data.transferEncodings
{
	public abstract class TransferEncoding
	{
		public abstract string Name { get; }

		public abstract long GetContentLength(byte[] content);

		public abstract void Write(Stream stream, byte[] content);
	}
}