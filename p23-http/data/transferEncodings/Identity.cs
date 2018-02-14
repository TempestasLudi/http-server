using System.IO;

namespace com.tempestasludi.c.p23_http.data.transferEncodings
{
	public class Identity : TransferEncoding
	{
		public override string Name => "identity";

		public override long GetContentLength(byte[] content)
		{
			return (content?.Length).GetValueOrDefault(0);
		}

		public override void Write(Stream stream, byte[] content)
		{
			if (content != null)
			{
				stream.Write(content, 0, content.Length);
			}
		}
	}
}