using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using com.tempestasludi.c.http_source.data;

namespace com.tempestasludi.c.http_source.util
{
  /// <summary>
  /// Relays a connection between a HTTP client and a HTTP server.
  /// </summary>
  public class Proxy
  {
    /// <summary>
    /// The name of the server to connect to.
    /// </summary>
    private readonly string _host;
    
    /// <summary>
    /// The server port to connect to.
    /// </summary>
    private readonly int _hostPort;
    
    /// <summary>
    /// Whether the proxy should set the "host" field of a request before sending it on to the server.
    /// </summary>
    private readonly bool _setHost;

    /// <summary>
    /// Creates a new proxy.
    /// </summary>
    /// <param name="host">The name of the server to connect to.</param>
    /// <param name="hostPort">The server port to connect to.</param>
    /// <param name="setHost">Whether the proxy should set the "host" field of a request before sending it on to the server.</param>
    public Proxy(string host, int hostPort = 80, bool setHost = false)
    {
      _host = host;
      _setHost = setHost;
      _hostPort = hostPort;
    }

    /// <summary>
    /// Takes a client's request and their connection and starts relaying a connection between them and the server.
    /// </summary>
    /// <param name="clientStream">The connection of the client.</param>
    /// <param name="request">The request of the client.</param>
    public void Run(Stream clientStream, Request request)
    {
      using (var serverClient = new TcpClient())
      {
        serverClient.Connect(_host, _hostPort);
        var serverStream = serverClient.GetStream();

        if (_setHost)
        {
          request.Host = _host;
        }

        request.Write(serverStream);

        var threads = new List<Thread>();
        var halter = new Thread(() => threads.ForEach(t => t.Abort()));

        threads = new(Stream, Stream)[] {(clientStream, serverStream), (serverStream, clientStream)}
          .Select(streams => new Thread(() =>
            {
              var (from, to) = streams;
              var buffer = new byte[1024];
              while (true)
              {
                try
                {
                  var length = from.Read(buffer, 0, buffer.Length);
                  if (length == 0)
                  {
                    break;
                  }

                  if (length > 0)
                  {
                    to.Write(buffer, 0, length);
                  }
                }
                catch (Exception e)
                {
                  if (e is IOException || e is ObjectDisposedException)
                  {
                    break;
                  }
                }
              }
              halter.Start();
            }
          )).ToList();
        threads.ForEach(thread => thread.Start());
        threads.ForEach(thread => thread.Join());
      }
    }
  }
}