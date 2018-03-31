using System;
using System.IO;
using System.Threading;

namespace com.tempestasludi.c.http_source.websockets
{
	public static class WebSocketListener
	{
		public static Thread StartListening(Stream stream, Action<byte[], byte> handler)
		{
			var result = new Thread(() =>
			{
				while (true)
				{
					var message = WebSocketTransciever.ReadMessage(stream, out var opcode);
					switch (opcode)
					{
						case WebSocketTransciever.OpcodeBinary:
						case WebSocketTransciever.OpcodeText:
						case WebSocketTransciever.OpcodeClose:
							handler(message, opcode);
							if (opcode == WebSocketTransciever.OpcodeClose)
							{
								Thread.CurrentThread.Abort();
							}
							break;
						case WebSocketTransciever.OpcodePing:
							WebSocketTransciever.SendMessage(stream, new byte[0], WebSocketTransciever.OpcodePong);
							break;
					}
				}
			});
			result.Start();
			return result;
		}
	}
}