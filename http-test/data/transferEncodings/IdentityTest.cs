using System.IO;
using System.Text;
using com.tempestasludi.c.http_source.data.transferEncodings;
using NUnit.Framework;
using Moq;

namespace com.tempestasludi.c.http_test.data.transferEncodings
{
  [TestFixture]
  public class IdentityTest
  {
    private static Identity CreateIdentity()
    {
      return new Identity();
    }
    
    public class GetContentLengthTest : IdentityTest
    {
      [Test]
      public void TestNull()
      {
        var identity = CreateIdentity();
        Assert.AreEqual(0, identity.GetContentLength(null));
      }
      
      [TestCase(new byte[] {})]
      [TestCase(new byte[] {5})]
      [TestCase(new byte[] {1, 55, 32, 42, 13, 66, 137, 55, 1, 253, 37, 53, 137, 15, 0})]
      [Test]
      public void TestArray(byte[] array)
      {
        var identity = CreateIdentity();
        Assert.AreEqual(array.Length, identity.GetContentLength(array));
      }
      
      [Test]
      public void TestLongString()
      {
        var identity = CreateIdentity();
        Assert.AreEqual(127,
          identity.GetContentLength(Encoding.ASCII.GetBytes(
            "0123456789112345678921234567893123456789412345678951234567896123456789712345678981234567899123456789012345678911234567892123456")));
      }
    }

    public class WriteTest : IdentityTest
    {
      private static Mock<Stream> MockStream()
      {
        var stream = new Mock<Stream>();
        return stream;
      }

      [Test]
      public void TestNull()
      {
        var identity = CreateIdentity();
        var streamMock = MockStream();
        identity.Write(streamMock.Object, null);
        streamMock.Verify(mock => mock.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
      }

      [TestCase(new byte[] {})]
      [TestCase(new byte[] {5})]
      [TestCase(new byte[] {1, 55, 32, 42, 13, 66, 137, 55, 1, 253, 37, 53, 137, 15, 0})]
      [Test]
      public void TestArray(byte[] array)
      {
        var identity = CreateIdentity();
        var streamMock = MockStream();
        identity.Write(streamMock.Object, array);
        streamMock.Verify(mock => mock.Write(array, 0, array.Length), Times.Once);
      }
      
      [Test]
      public void TestLongString()
      {
        var identity = CreateIdentity();
        var streamMock = MockStream();
        var array = Encoding.ASCII.GetBytes(
            "0123456789112345678921234567893123456789412345678951234567896123456789712345678981234567899123456789012345678911234567892123456");
        identity.Write(streamMock.Object, array);
        streamMock.Verify(mock => mock.Write(array, 0, array.Length), Times.Once);
      }
      
    }

    public class PropertiesTest : IdentityTest
    {
      [Test]
      public void TestName()
      {
        var identity = CreateIdentity();
        Assert.AreEqual("identity", identity.Name);
      }
    }
  }
}