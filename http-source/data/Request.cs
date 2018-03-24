using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace com.tempestasludi.c.p23_http.data
{
	public class Request : Message
	{
		public string Method;
		public string Uri;
		
		public List<Cookie> Cookies = new List<Cookie>();
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
					{"cookie", (request, cookies) => request.Cookies = cookies.Split(";".ToCharArray()).Select(s => Cookie.Read(s.Trim())).ToList()},
					{"host", (request, host) => request.Host = host},
					{"origin", (request, origin) => request.Origin = origin}
				}
			);
		}

		protected override string GetHeaderLine()
		{
			return $"{Method} {Uri} {Protocol}";
		}

		protected override List<(string, string)> GetHeaders()
		{
			var headers = base.GetHeaders();
			if (Cookies.Count > 0) headers.Add(("Cookie", Cookies.Select(cookie => cookie.ToString()).Aggregate((a, b) => $"{a}; {b}")));
			if (Host != null) headers.Add(("Host", Host));
			if (Origin != null) headers.Add(("Origin", Origin));
			return headers;
		}
	}
}