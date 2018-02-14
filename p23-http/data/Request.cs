using System;
using System.Collections.Generic;
using System.IO;

namespace com.tempestasludi.c.p23_http.data
{
	public class Request : Message
	{
		public string Method;
		public string Uri;
		public string Host;
		public string Origin;

		public static Request Read(Stream stream)
		{
			return Read(
				stream,
				(request, parts) =>
				{
					request.Method = parts[0];
					request.Uri = parts[1];
					request.Protocol = parts[2];
				},
				new Dictionary<string, Action<Request, string>>
				{
					{"host", (request, host) => request.Host = host},
					{"origin", (request, origin) => request.Origin = origin}
				}
			);
		}

		protected override string GetHeaderLine()
		{
			return $"{Method} {Uri} {Protocol}";
		}

		protected override Dictionary<string, string> GetHeaders()
		{
			var headers = base.GetHeaders();
			if (Host != null) headers["Host"] = Host;
			if (Origin != null) headers["Origin"] = Origin;
			return headers;
		}
	}
}