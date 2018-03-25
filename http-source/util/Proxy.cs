using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using com.tempestasludi.c.http_source.data;

namespace com.tempestasludi.c.http_source.util
{
  public class Proxy
  {
    private readonly string _host;
    private readonly int _hostPort;
    private readonly bool _setHost;

    public Proxy(string host, int hostPort = 80, bool setHost = false)
    {
      _host = host;
      _setHost = setHost;
      _hostPort = hostPort;
    }

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