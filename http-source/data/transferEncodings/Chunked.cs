using System;
using System.IO;
using System.Text;

namespace com.tempestasludi.c.http_source.data.transferEncodings
{
	public class Chunked : TransferEncoding
	{
		public Func<byte[]> DataSupplier = () => new byte[0];


		public override string Name => "chunked";
		
		public override long GetContentLength(byte[] content)
		{
			return 0;
		}

		public override void Write(Stream stream, byte[] content)
		{
			byte[] data;
			var trailerBytes = Encoding.UTF8.GetBytes("\r\n");
			do
			{
				data = DataSupplier();
				var headerBytes = Encoding.UTF8.GetBytes(data.Length + "\r\n");
				stream.Write(headerBytes, 0, headerBytes.Length);
				stream.Write(data, 0, data.Length);
				stream.Write(trailerBytes, 0, trailerBytes.Length);
				stream.Flush();
			} while (data.Length > 0);
		}
	}
}