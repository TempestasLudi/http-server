using System;
using System.Collections.Generic;
using System.IO;

namespace com.tempestasludi.c.p23_http.data
{
	public class Response : Message
	{
		public int StatusCode;
		public string Status;
		
		public readonly List<Cookie> Cookies = new List<Cookie>();
		public string Server = "Tempestaserver";

		public static Response Read(Stream stream)
		{
			return Read(
				stream,
				(response, parts) =>
				{
					response.Protocol = parts[0];
					response.StatusCode = int.Parse(parts[1]);
					response.Status = parts[2];
				},
				new Dictionary<string, Action<Response, string>>
				{
					{"set-cookie", (response, cookie) => response.Cookies.Add(Cookie.Read(cookie))},
					{"server", (response, server) => response.Server = server}
				}
			);
		}

		protected override string GetHeaderLine()
		{
			return $"{Protocol} {StatusCode} {Status}";
		}

		protected override List<(string, string)> GetHeaders()
		{
			var headers = base.GetHeaders();
			Cookies.ForEach(cookie => headers.Add(("Set-Cookie", cookie.ToString())));
			headers.Add(("Server", Server));
			return headers;
		}
	}
}