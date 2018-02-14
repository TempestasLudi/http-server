using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using com.tempestasludi.c.p23_http.data.transferEncodings;

namespace com.tempestasludi.c.p23_http.data
{
	public abstract class Message
	{
		public string Protocol = "HTTP/1.1";
		public Dictionary<string, string> Headers = new Dictionary<string, string>();

		public byte[] Content;
		
		public long ContentLength => TransferEncoding.GetContentLength(Content);
		public string Connection;
		public string ContentType;
		public DateTime Date = DateTime.UtcNow;
		public string Upgrade;
		public TransferEncoding TransferEncoding = new Identity();

		private static string ReadLine(Stream stream)
		{
			var builder = new StringBuilder();
			do
			{
				builder.Append((char) stream.ReadByte());
			} while (builder.Length < 2 || builder[builder.Length - 2] != '\r' || builder[builder.Length - 1] != '\n');
			builder.Remove(builder.Length - 2, 2);
			return builder.ToString();
		}

		private static Dictionary<string, string> ReadHeaders(Stream stream, Dictionary<string, Action<string>> headerActions)
		{
			var headers = new Dictionary<string, string>();
			string[] lineParts;
			do
			{
				lineParts = ReadLine(stream).Split(new[] {':'}, 2).Select(a => a.Trim()).ToArray();
				if (lineParts.Length < 2)
				{
					break;
				}
				lineParts[0] = lineParts[0].ToLower();
				if (headerActions.ContainsKey(lineParts[0]))
				{
					headerActions[lineParts[0]](lineParts[1]);
				}
				else
				{
					headers[lineParts[0]] = lineParts[1];
				}
			} while (lineParts.Length > 1);
			return headers;
		}

		protected static T Read<T>(Stream stream, Action<T, string[]> headerLineAction,
			Dictionary<string, Action<T, string>> headerActions) where T : Message, new()
		{
			var result = new T();

			headerLineAction(result, ReadLine(stream).Split(new[] {' '}, 3));

			var contentLength = 0;

			result.Headers = ReadHeaders(stream,
				headerActions.Select(pair => new KeyValuePair<string, Action<string>>(pair.Key, value => pair.Value(result, value)))
					.Concat(new Dictionary<string, Action<string>>
					{
						{"connection", value => result.Connection = value},
						{"content-length", value => contentLength = int.Parse(value)},
						{"content-type", value => result.ContentType = value},
						{"date", value => result.Date = DateTime.Parse(value)},
						{"upgrade", value => result.Upgrade = value}
					}).ToDictionary(pair => pair.Key, pair => pair.Value)
			);

			if (contentLength <= 0) return result;

			var buffer = new byte[contentLength];
			stream.Read(buffer, 0, contentLength);
			result.Content = buffer;

			return result;
		}

		protected abstract string GetHeaderLine();

		protected virtual Dictionary<string, string> GetHeaders()
		{
			var headers = new Dictionary<string, string>(Headers);
			if (Connection != null) headers["Connection"] = Connection;
			if (ContentLength > 0) headers["Content-Length"] = ContentLength.ToString();
			if (ContentType != null) headers["Content-Type"] = ContentType;
			headers["Date"] = Date.ToString("r");
			if (Upgrade != null) headers["Upgrade"] = Upgrade;
			if (TransferEncoding.GetType() != typeof(Identity)) headers["Transfer-Encoding"] = TransferEncoding.Name;
			return headers;
		}

		private byte[] GetHeaderBytes()
		{
			return Encoding.UTF8
				.GetBytes(
					GetHeaderLine() + GetHeaders().Select(pair => $"{pair.Key}: {pair.Value}").Aggregate("", (a, b) => $"{a}\r\n{b}") +
					"\r\n\r\n").ToArray();
		}

		public void Write(Stream stream)
		{
			var buffer = GetHeaderBytes();
			stream.Write(buffer, 0, buffer.Length);
			TransferEncoding.Write(stream, Content);
		}
	}
}