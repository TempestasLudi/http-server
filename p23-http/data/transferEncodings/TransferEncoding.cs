using System.IO;

namespace com.tempestasludi.c.p23_http.data.transferEncodings
{
	public abstract class TransferEncoding
	{
		public abstract string Name { get; }

		public abstract long GetContentLength(byte[] content);

		public abstract void Write(Stream stream, byte[] content);
	}
}