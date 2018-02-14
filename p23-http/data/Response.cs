using System;
using System.Collections.Generic;
using System.IO;

namespace com.tempestasludi.c.p23_http.data
{
	public class Response : Message
	{
		public int StatusCode;
		public string Status;
		public string Server = "Tempestaserver";

		public Response Read(Stream stream)
		{
			return Read(
				stream,
				(response, parts) =>
				{
					response.Protocol = parts[0];
					response.StatusCode = int.Parse(parts[1]);
					response.Status = parts[2];
				},
				new Dictionary<string, Action<Response, string>>()
			);
		}

		protected override string GetHeaderLine()
		{
			return $"{Protocol} {StatusCode} {Status}";
		}

		protected override Dictionary<string, string> GetHeaders()
		{
			var headers = base.GetHeaders();
			headers["Server"] = Server;
			return headers;
		}
	}
}