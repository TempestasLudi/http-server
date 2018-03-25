using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using com.tempestasludi.c.p23_http.data;

namespace com.tempestasludi.c.p23_http.util
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
      var isRunning = true;

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
        var halter = new Thread(() =>
        {
          isRunning = false;
          threads.ForEach(t => t.Abort());
        });
        threads = new(Stream, Stream)[] {(clientStream, serverStream), (serverStream, clientStream)}.Select(tuple =>
        {
          var from = tuple.Item1;
          var to = tuple.Item2;
          var thread = new Thread(() =>
          {
            var buffer = new byte[1024];
            while (isRunning)
            {
              try
              {
                var length = from.Read(buffer, 0, buffer.Length);
                to.Write(buffer, 0, length);
              }
              catch (IOException)
              {
                halter.Start();
                break;
              }
            }
          });
          thread.Start();
          return thread;
        }).ToList();
        threads.ForEach(thread => thread.Join());
      }
    }
  }
}