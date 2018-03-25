using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using com.tempestasludi.c.http_source.data;

namespace com.tempestasludi.c.http_source.actors
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

    private readonly List<Route> _routes = new List<Route>();

    public void AddRoute(Action<Stream, Request> requestHandler, Regex path=null, Regex host=null, bool closeConnection = true)
    {
      _routes.Add(new Route(path, host, requestHandler, closeConnection));
    }

    public void AddRoute(string file, Regex path, Regex host = null)
    {
      AddRoute((stream, request) =>
      {
        var match = path.Match(request.Uri);
        var subPath = request.Uri.Substring(match.Length);
        var location = Path.Combine(file, subPath);
        try
        {
          SendFile(stream, new[]
            {
              location,
              Path.Combine(location, "index.xml"),
              Path.Combine(location, "index.html")
            }.SkipWhile(f => !File.Exists(f))
            .First());
        }
        catch (InvalidOperationException)
        {
          new Response
          {
            Content = Encoding.UTF8.GetBytes("<html><body><h1>Not Found</h1></body></html>"),
            ContentType = "text/html",
            Status = "Not Found",
            StatusCode = 404
          }.Write(stream);
        }
      }, path, host);
    }

    private static void SendFile(Stream stream, string path)
    {
      var extension = Path.GetExtension(path)?.Substring(1) ?? "";
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
      var uri = request.Uri;
      var host = request.Host ?? "";
      var usedRoute = _routes.SkipWhile(route => route.PathRegex.IsMatch(uri) && route.HostRegex.IsMatch(host))
        .FirstOrDefault();

      if (usedRoute != null)
      {
        usedRoute?.RequestHandler(stream, request);
        return usedRoute.CloseConnection;
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

  internal class Route
  {
    public readonly Regex PathRegex;
    public readonly Regex HostRegex;
    public readonly Action<Stream, Request> RequestHandler;
    public readonly bool CloseConnection;

    public Route(Regex pathRegex, Regex hostRegex, Action<Stream, Request> requestHandler, bool closeConnection)
    {
      PathRegex = pathRegex ?? new Regex(".*");
      HostRegex = hostRegex ?? new Regex(".*");
      RequestHandler = requestHandler;
      CloseConnection = closeConnection;
    }
  }
}