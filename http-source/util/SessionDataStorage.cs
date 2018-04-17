using System;
using System.Collections.Generic;
using System.Linq;

namespace com.tempestasludi.c.http_source.util
{
  /// <summary>
  ///   Stores data that is related to a session, each session linked to a guid.
  /// </summary>
  public class SessionDataStorage
  {
    /// <summary>
    ///   The time before a session expires.
    /// </summary>
    private readonly TimeSpan _expirationSpan;

    /// <summary>
    ///   The dictionary for finding sessions based on their id.
    /// </summary>
    private Dictionary<Guid, Session> _sessions = new Dictionary<Guid, Session>();

    /// <summary>
    ///   Creates a new session storage.
    /// </summary>
    /// <param name="expirationSpan">The time before a session expires.</param>
    public SessionDataStorage(TimeSpan? expirationSpan = null)
    {
      _expirationSpan = expirationSpan ?? new TimeSpan(0, 6, 0);
    }

    /// <summary>
    ///   Creates a new session.
    /// </summary>
    /// <returns>The session.</returns>
    public Session CreateSession()
    {
      var session = new Session(DateTime.Now.Add(_expirationSpan));
      _sessions[session.Id] = session;
      return session;
    }

    /// <summary>
    ///   Retrieves a session based on an id.
    /// </summary>
    /// <param name="id">The id of the session to retrieve.</param>
    /// <returns>The session with the specified id.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no session exists with the specified id.</exception>
    public Session GetSession(Guid id)
    {
      if (_sessions.ContainsKey(id)) return _sessions[id];

      throw new KeyNotFoundException("No session with given id exists");
    }

    /// <summary>
    ///   Retrieves a session based on an id string.
    /// </summary>
    /// <param name="id">The id string of the session to retrieve.</param>
    /// <returns>The session with the specified id.</returns>
    public Session GetSession(string id)
    {
      return GetSession(Guid.Parse(id));
    }

    /// <summary>
    ///   Deletes the expired sessions.
    /// </summary>
    public void Cleanup()
    {
      _sessions = _sessions.Where(entry => entry.Value.Expiration >= DateTime.Now)
        .ToDictionary(s => s.Key, s => s.Value);
    }
  }

  /// <summary>
  ///   Maintains the data of a session.
  /// </summary>
  public class Session
  {
    /// <summary>
    ///   The objects, associated with a string name, that constitute the data of the session.
    /// </summary>
    public readonly Dictionary<string, object> Data = new Dictionary<string, object>();

    /// <summary>
    ///   The id of the session.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();

    /// <summary>
    ///   The date when the session expires.
    /// </summary>
    public DateTime Expiration;

    public Session(DateTime expiration)
    {
      Expiration = expiration;
    }

    /// <summary>
    ///   Postpones the expiration of the session.
    /// </summary>
    /// <param name="span">The time span between now and the expiration moment.</param>
    public void PostponeExpiration(TimeSpan span)
    {
      Expiration = DateTime.Now.Add(span);
    }

    /// <summary>
    ///   Checks equality between this session and another session.
    /// </summary>
    /// <param name="other">The session to check equality with.</param>
    /// <returns>Whether this session equals the other session.</returns>
    protected bool Equals(Session other)
    {
      return Id.Equals(other.Id);
    }

    /// <summary>
    ///   Checks equality between this session and another object.
    /// </summary>
    /// <param name="obj">The object to check equality with.</param>
    /// <returns>Whether this session equals the other object.</returns>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((Session) obj);
    }

    /// <summary>
    ///   Generates the hash code for this session.
    /// </summary>
    /// <returns>The hash code for this session.</returns>
    public override int GetHashCode()
    {
      return Id.GetHashCode();
    }
  }
}
