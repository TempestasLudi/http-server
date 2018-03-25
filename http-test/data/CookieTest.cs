using System;
using com.tempestasludi.c.http_source.data;
using NUnit.Framework;

namespace com.tempestasludi.c.http_test.data
{
	[TestFixture]
	public class CookieTest
	{
		public class ToStringTest : CookieTest
		{
			[Test]
			public void TestSimple()
			{
				Assert.AreEqual(
					"Session=1337",
					new Cookie("Session", "1337").ToString()
				);
			}

			[Test]
			public void TestBooleans()
			{
				Assert.AreEqual(
					"Session=1337; Secure; HttpOnly",
					new Cookie("Session", "1337", secure: true, httpOnly: true).ToString()
				);
			}

			[Test]
			public void TestScoping()
			{
				Assert.AreEqual(
					"Session=1337; Domain=.example.com; Path=/",
					new Cookie("Session", "1337", ".example.com", "/").ToString()
				);
			}

			[Test]
			public void TestBooleansAndScoping()
			{
				Assert.AreEqual(
					"Session=1337; Domain=.example.com; Secure", 
					new Cookie("Session", "1337", ".example.com", secure: true, httpOnly: false).ToString()
				);
			}

			[Test]
			public void TestDate()
			{
				Assert.AreEqual(
					"Session=1337; Domain=.example.com; Path=/; Expires=Wed, 29 Jun 1729 23:00:00 GMT; Max-Age=86400; Secure; HttpOnly",
					new Cookie("Session", "1337", ".example.com", "/", new DateTime(1729, 06, 30), 60 * 60 * 24, true, true).ToString()
				);
			}
		}
		
		public class ReadTest : CookieTest
		{
			[Test]
			public void TestSimple()
			{
				Assert.AreEqual(
					new Cookie("Session", "1337"),
					Cookie.Read("Session=1337")
				);
			}

			[Test]
			public void TestBooleans()
			{
				Assert.AreEqual(
					new Cookie("Session", "1337", secure: true, httpOnly: true),
					Cookie.Read("Session=1337; Secure; HttpOnly")
				);
			}

			[Test]
			public void TestScoping()
			{
				Assert.AreEqual(
					new Cookie("Session", "1337", ".example.com", "/"),
					Cookie.Read("Session=1337; Domain=.example.com; Path=/")
				);
			}

			[Test]
			public void TestBooleansAndScoping()
			{
				Assert.AreEqual(
					new Cookie("Session", "1337", ".example.com", secure: true),
					Cookie.Read("Session=1337; Domain=.example.com; Secure")
				);
			}

			[Test]
			public void TestDate()
			{
				Assert.AreEqual(
					new Cookie("Session", "1337", ".example.com", "/", new DateTime(1729, 06, 30), 60 * 60 * 24, true, true),
					Cookie.Read("Session=1337; Domain=.example.com; Path=/; Expires=Wed, 29 Jun 1729 23:00:00 GMT; Max-Age=86400; Secure; HttpOnly")
				);
			}
		}
	}
}
