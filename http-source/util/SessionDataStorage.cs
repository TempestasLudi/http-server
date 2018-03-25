using System;
using System.Collections.Generic;
using System.Linq;

namespace com.tempestasludi.c.http_source.util
{
  
  /// <summary>
  /// Stores data that is related to a session, each session linked to a guid.
  /// </summary>
  public class SessionDataStorage
  {
    /// <summary>
    /// The dictionary for finding sessions based on their id.
    /// </summary>
    private Dictionary<Guid, Session> _sessions = new Dictionary<Guid, Session>();

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <returns>The session.</returns>
    private Session CreateSession()
    {
      var session = new Session();
      _sessions[session.Id] = session;
      return session;
    }

    /// <summary>
    /// Retrieves a session based on an id.
    /// </summary>
    /// <param name="id">The id of the session to retrieve.</param>
    /// <returns>The session with the specified id.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no session exists with the specified id.</exception>
    private Session GetSession(Guid id)
    {
      if (_sessions.ContainsKey(id))
      {
        return _sessions[id];
      }
      throw new KeyNotFoundException("No session with given id exists");
    }

    /// <summary>
    /// Retrieves a session based on an id string.
    /// </summary>
    /// <param name="id">The id string of the session to retrieve.</param>
    /// <returns>The session with the specified id.</returns>
    private Session GetSession(string id)
    {
      return GetSession(Guid.Parse(id));
    }

    /// <summary>
    /// Deletes the expired sessions.
    /// </summary>
    public void Cleanup()
    {
      _sessions = _sessions.Where(entry => entry.Value.Expiration <= DateTime.Now).ToDictionary(s => s.Key, s => s.Value);
    }
  }

  /// <summary>
  /// Maintains the data of a session.
  /// </summary>
  public class Session
  {
    /// <summary>
    /// The id of the session.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();
    
    /// <summary>
    /// The date when the session expires.
    /// </summary>
    public DateTime Expiration = DateTime.Now.AddHours(6);
    
    /// <summary>
    /// The objects, associated with a string name, that constitute the data of the session.
    /// </summary>
    public readonly Dictionary<string, object> Data = new Dictionary<string, object>();

    /// <summary>
    /// Postpones the expiration of the session to a number of hours from now.
    /// </summary>
    /// <param name="hourCount">The number of hours to postpone the expiration with.</param>
    public void PostponeExpiration(int hourCount = 6)
    {
      Expiration = DateTime.Now.AddHours(hourCount);
    }
  }
}