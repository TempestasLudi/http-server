using System;
using System.Collections.Generic;
using com.tempestasludi.c.http_source.util;
using NUnit.Framework;

namespace com.tempestasludi.c.http_test.util
{
  [TestFixture]
  public class SessionDataStorageTest
  {
    public class CreateSessionTest : SessionDataStorageTest
    {
      [Test]
      public void TestNotNull()
      {
        var storage = new SessionDataStorage();
        var session = storage.CreateSession();
        Assert.NotNull(session);
      }

      [Test]
      public void TestUnique()
      {
        var storage = new SessionDataStorage();
        var firstSession = storage.CreateSession();
        var secondSession = storage.CreateSession();
        Assert.AreNotSame(firstSession, secondSession);
        Assert.AreNotEqual(firstSession, secondSession);
      }
    }

    public class GetSessionTest : SessionDataStorageTest
    {
      [Test]
      public void TestNotNull()
      {
        var storage = new SessionDataStorage();
        var session = storage.CreateSession();
        var retrievedSession = storage.GetSession(session.Id);
        Assert.NotNull(retrievedSession);
      }
      
      [Test]
      public void TestSame()
      {
        var storage = new SessionDataStorage();
        
        for (var i = 0; i < 3; i++)
        {
          storage.CreateSession();
        }

        var session = storage.CreateSession();
        
        for (var i = 0; i < 7; i++)
        {
          storage.CreateSession();
        }

        var retrievedSession = storage.GetSession(session.Id);
        Assert.AreSame(session, retrievedSession);
      }

      [Test]
      public void TestException()
      {
        var storage = new SessionDataStorage();
        var session = storage.CreateSession();
        var newKey = new Guid();
        Assert.Throws<KeyNotFoundException>(() => storage.GetSession(newKey));
      }
      
      [Test]
      public void TestGetString()
      {
        var storage = new SessionDataStorage();
        
        for (var i = 0; i < 3; i++)
        {
          storage.CreateSession();
        }

        var session = storage.CreateSession();
        
        for (var i = 0; i < 7; i++)
        {
          storage.CreateSession();
        }

        var retrievedSession = storage.GetSession(session.Id.ToString());
        Assert.AreSame(session, retrievedSession);
      }
    }

    public class CleanupTest : SessionDataStorageTest
    {
      [Test]
      public void TestRemaining()
      {
        var storage = new SessionDataStorage();
        var session = storage.CreateSession();
        storage.Cleanup();
        var retrievedSession = storage.GetSession(session.Id);
        Assert.AreSame(session, retrievedSession);
      }
      
      [Test]
      public void TestRemoval()
      {
        var storage = new SessionDataStorage();
        var session = storage.CreateSession();
        session.Expiration = DateTime.Now.Subtract(new TimeSpan(1));
        var retrievedSession = storage.GetSession(session.Id);
        Assert.AreSame(session, retrievedSession);
        storage.Cleanup();
        Assert.Throws<KeyNotFoundException>(() => storage.GetSession(session.Id));
      }
    }
  }
}