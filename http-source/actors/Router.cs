using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using com.tempestasludi.c.p23_http.data;

namespace com.tempestasludi.c.p23_http.actors
{
	public class Router
	{
		private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>
		{
			{"txt", "text/plain"},
			{"json", "application/json"},
			{"xml", "application/xml"},
			{"html", "text/html"},
			{"js", "application/javascript"},
			{"css", "text/css"},
			{"xsl", "text/xsl"}
		};

		private readonly List<Tuple<Regex, Action<Stream, Request>, bool>> _routes =
			new List<Tuple<Regex, Action<Stream, Request>, bool>>();

		public void AddRoute(Regex path, Action<Stream, Request> requestHandler, bool closeConnection = true)
		{
			_routes.Add(Tuple.Create(path, requestHandler, closeConnection));
		}

		public void AddRoute(Regex path, string file)
		{
			AddRoute(path, (stream, request) =>
				{
					var match = path.Match(request.Uri);
					var subPath = request.Uri.Substring(match.Length);
					var location = Path.Combine(file, subPath);
					if (File.Exists(location))
					{
						SendFile(stream, location);
					}
					else if (File.Exists(Path.Combine(location, "index.xml")))
					{
						SendFile(stream, Path.Combine(location, "index.xml"));
					}
					else if (File.Exists(Path.Combine(location, "index.html")))
					{
						SendFile(stream, Path.Combine(location, "index.html"));
					}
					else
					{
						new Response
						{
							Content = Encoding.UTF8.GetBytes("<html><body><h1>Not Found</h1></body></html>"),
							ContentType = "text/html",
							Status = "Not Found",
							StatusCode = 404
						}.Write(stream);
					}
				}
			);
		}

		private static void SendFile(Stream stream, string path)
		{
			var extension = Path.GetExtension(path).Substring(1);
			new Response
			{
				Content = File.ReadAllBytes(path),
				ContentType = MimeTypes.ContainsKey(extension) ? MimeTypes[extension] : null,
				Status = "OK",
				StatusCode = 200
			}.Write(stream);
		}

		public bool Route(Stream stream, Request request)
		{
			var usedRoute = _routes.Where(route =>
			{
				var match = route.Item1.Match(request.Uri);
				return match.Success && request.Uri.Substring(0, match.Length) == match.Value;
			}).FirstOrDefault();

			if (usedRoute != null)
			{
				usedRoute.Item2(stream, request);
				return usedRoute.Item3;
			}

			new Response
			{
				Content = Encoding.UTF8.GetBytes("<html><body><h1>Not Found</h1></body></html>"),
				ContentType = "text/html",
				Status = "Not Found",
				StatusCode = 404
			}.Write(stream);

			return true;
		}
	}
}