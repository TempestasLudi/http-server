using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using com.tempestasludi.c.p23_http.data;

namespace com.tempestasludi.c.p23_http.websockets
{
	public static class WebSocketTransciever
	{
		private const byte OpcodeContinuation = 0x0;
		public const byte OpcodeText = 0x1;
		public const byte OpcodeBinary = 0x2;
		public const byte OpcodeClose = 0x8;
		public const byte OpcodePing = 0x9;
		public const byte OpcodePong = 0xa;

		private const ulong MaxFrameLength = ulong.MaxValue;

		public static void SendHandshake(Stream stream, Request request)
		{
			new Response
			{
				Connection = "Upgrade",
				Upgrade = "websocket",
				StatusCode = 101,
				Status = "Switching Protocols",
				Headers =
				{
					["Sec-Websocket-Accept"] = Convert.ToBase64String(new SHA1CryptoServiceProvider().ComputeHash(
						Encoding.ASCII.GetBytes(request.Headers["sec-websocket-key"] + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")))
				}
			}.Write(stream);
		}

		public static void SendMessage(Stream stream, byte[] content, byte opcode = OpcodeText)
		{
			var numFrames = (int) Math.Ceiling((float) content.Length / MaxFrameLength);
			var length = (ulong) content.Length;
			Enumerable.Range(0, numFrames).Select(i => (ulong) i).ToList().ForEach(i =>
			{
				var blockLength = Math.Min(MaxFrameLength, length - i * MaxFrameLength);
				var block = new byte[blockLength];
				Array.Copy(content, (long) (i * MaxFrameLength), block, 0, (long) blockLength);
				SendFrame(stream, block, i == (ulong) (numFrames - 1), i == 0 ? opcode : OpcodeContinuation);
			});
		}

		private static void SendFrame(Stream stream, byte[] content, bool isLast, byte opcode)
		{
			var builder = new MemoryStream();
			builder.Write(new[] {(byte) ((isLast ? 1 << 7 : 0) | opcode)}, 0, 1);

			if (content.Length < 126)
			{
				builder.Write(new[] {(byte) content.Length}, 0, 1);
			}
			else if (content.Length < ushort.MaxValue)
			{
				builder.Write(new[] {(byte) 126, (byte) (content.Length >> 8), (byte) content.Length}, 0, 3);
			}
			else
			{
				builder.Write(new[] {(byte) 127}, 0, 1);
				var length = (ulong) content.Length;
				builder.Write(Enumerable.Range(0, 8).Select(i => (byte) (length >> (8 * i))).Reverse().ToArray(), 0, 8);
			}

			builder.Write(content, 0, content.Length);
			stream.Write(builder.ToArray(), 0, (int) builder.Length);
		}

		public static byte[] ReadMessage(Stream stream, out byte opcode)
		{
			var builder = new MemoryStream();
			opcode = 255;
			bool done;

			do
			{
				var firstByte = stream.ReadByte();
				if (opcode == 255)
				{
					opcode = (byte) (firstByte & 0xf);
				}

				done = firstByte >> 7 == 1;

        var frameContent = ReadFrame(stream);
        builder.Write(frameContent, 0, frameContent.Length);
			} while (!done);

			return builder.ToArray();
		}

		private static byte[] ReadFrame(Stream stream)
		{
			var length = (ulong) stream.ReadByte() ^ (1 << 7);

			if (length == 126)
			{
				length = (ulong) ((stream.ReadByte() << 8) + stream.ReadByte());
			}
			else if (length == 127)
			{
				length = 0;
				for (var i = 0; i < 8; i++)
				{
					length = (length << 8) + (ulong) stream.ReadByte();
				}
			}

			var mask = new byte[4];
			var content = new byte[length];

			stream.Read(mask, 0, 4);
			stream.Read(content, 0, (int) length);

			for (ulong i = 0; i < length; i++)
			{
				content[i] ^= mask[i % 4];
			}

			return content;
		}
	}
}