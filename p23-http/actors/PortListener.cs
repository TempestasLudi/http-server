using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using com.tempestasludi.c.p23_http.data;

namespace com.tempestasludi.c.p23_http.actors
{
	public class PortListener
	{
		private readonly int _port;
		private readonly IPAddress _address = IPAddress.Any;
		private readonly Func<Stream, Request, bool> _requestProcessor;

		private Thread _thread;
		private bool _isRunning;

		public PortListener(int port, Func<Stream, Request, bool> requestProcessor)
		{
			_port = port;
			_requestProcessor = requestProcessor;
		}

		public PortListener(int port, Func<Request, Response> requestProcessor) : this(port,
			(stream, request) =>
			{
				requestProcessor(request).Write(stream);
				return true;
			})
		{
		}

		public void Start()
		{
			_isRunning = true;
			_thread = new Thread(Run);
			_thread.Start();
		}

		private void Run()
		{
			var listener = new TcpListener(new IPEndPoint(_address, _port));
			listener.Start();
			while (_isRunning)
			{
				var socket = listener.AcceptSocket();
				new Thread(() =>
				{
					var stream = new NetworkStream(socket);
					var request = Request.Read(stream);
					try
					{
						if (_requestProcessor(stream, request))
						{
							socket.Disconnect(false);
						}
					}
					catch (IOException)
					{}
				}).Start();
			}
		}

		public void Stop()
		{
			_isRunning = false;
			_thread.Abort();
		}
	}
}