using System;
using System.Collections.Generic;

namespace com.tempestasludi.c.p23_http.util
{
  public class SessionDataStorage
  {
    private Dictionary<Guid, Dictionary<string, object>> sessions = new Dictionary<Guid, Dictionary<string, object>>();

    private (Guid, Dictionary<string, object>) CreateSession()
    {
      var id = Guid.NewGuid();
      if (sessions.ContainsKey(id))
      {
        throw new Exception("Cannot create session. Duplicate Guid.");
      }
      sessions[id] = new Dictionary<string, object>();
      return (id, sessions[id]);
    }

    // TODO: Error handling
    private Dictionary<string, object> GetSession(Guid id)
    {
      if (sessions.ContainsKey(id))
      {
        return sessions[id];
      }
      throw new Exception("No session with given id exists");
    }

    private Dictionary<string, object> GetSession(string id)
    {
      return GetSession(Guid.Parse(id));
    }
  }
}