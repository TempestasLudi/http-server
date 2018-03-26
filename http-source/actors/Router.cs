using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using com.tempestasludi.c.http_source.data;

namespace com.tempestasludi.c.http_source.actors
{
  public class Router
  {
    private readonly Dictionary<string, string> _mimeTypes;

    private readonly List<Route> _routes = new List<Route>();

    private readonly RouterConfig _config;

    public Router(RouterConfig config)
    {
      _config = config;

      _mimeTypes = XDocument.Load(config.MimeTypesFile).Root?.Elements("type").ToDictionary(
        e => e.Element("extension")?.Value,
        e => e.Element("mime")?.Value
      );
      if (_mimeTypes == null)
      {
        throw new FileNotFoundException("The mime types file was not found");
      }
    }

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
          SendFile(stream, new List<string>{location}
            .Concat(_config.DefaultFiles.Select(df => Path.Combine(location, df)))
            .SkipWhile(f => !File.Exists(f))
            .First());
        }
        catch (InvalidOperationException)
        {
          SendFile(stream, Path.Combine(_config.ErrorPagesDirectory, "404.html"), (404, "Not Found"));
        }
      }, path, host);
    }

    private void SendFile(Stream stream, string path, (int, string)? status = null)
    {
      var extension = Path.GetExtension(path)?.Substring(1) ?? "";
      var (statusCode, statusName) = status ?? (200, "OK");
      new Response
      {
        Content = File.ReadAllBytes(path),
        ContentType = _mimeTypes.ContainsKey(extension) ? _mimeTypes[extension] : null,
        Status = statusName,
        StatusCode = statusCode
      }.Write(stream);
    }

    public bool Route(Stream stream, Request request)
    {
      var uri = request.Uri;
      var host = request.Host ?? "";

      var usedRoute = _routes.SkipWhile(route => !(route.PathRegex.IsMatch(uri) && route.HostRegex.IsMatch(host)))
        .FirstOrDefault();

      if (usedRoute != null)
      {
        usedRoute.RequestHandler(stream, request);
        return usedRoute.CloseConnection;
      }
      
      SendFile(stream, Path.Combine(_config.ErrorPagesDirectory, "404.html"), (404, "Not Found"));

      return true;
    }
  }

  public class RouterConfig
  {
    public string ErrorPagesDirectory;
    public string MimeTypesFile = "config/mime-types.xml";
    public string[] DefaultFiles = {
      "index.xml",
      "index.html"
    };

    public RouterConfig(string errorPagesDirectory)
    {
      ErrorPagesDirectory = errorPagesDirectory;
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