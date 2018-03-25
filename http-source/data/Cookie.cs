using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.tempestasludi.c.http_source.data
{
  public class Cookie
  {
    public string Name;
    public string Value;

    public string Domain;
    public string Path;
    public DateTime? Expires;
    public long? MaxAge;
    public bool? Secure;
    public bool? HttpOnly;

    public bool UrlEncode = true;

    public Cookie(string name, string value, string domain = null, string path = null, DateTime? expires = null,
      long? maxAge = null, bool? secure = null, bool? httpOnly = null)
    {
      Name = name;
      Value = value;
      Domain = domain;
      Path = path;
      Expires = expires;
      MaxAge = maxAge;
      Secure = secure;
      HttpOnly = httpOnly;
    }

    public static Cookie Read(string text)
    {
      var parts = text.Split(";".ToCharArray()).Select(p =>
      {
        var pieces = p.Trim().Split("=".ToCharArray(), 2);
        return (pieces[0], pieces.Length > 1 ? pieces[1] : null);
      }).ToArray();

      var resultCookie = new Cookie(HttpUtility.UrlDecode(parts[0].Item1), HttpUtility.UrlDecode(parts[0].Item2));

      var actions = new Dictionary<string, Action<string>>
      {
        {"Domain", domain => resultCookie.Domain = domain},
        {"Path", path => resultCookie.Path = path},
        {"Expires", expiration => resultCookie.Expires = DateTime.Parse(expiration)},
        {"Max-Age", maxAge => resultCookie.MaxAge = long.Parse(maxAge)},
        {"Secure", _ => resultCookie.Secure = true},
        {"HttpOnly", _ => resultCookie.HttpOnly = true}
      };

      parts.ToList().ForEach(p =>
      {
        if (actions.ContainsKey(p.Item1))
        {
          actions[p.Item1](p.Item2);
        }
      });

      return resultCookie;
    }

    public override string ToString()
    {
      return new List<(string, string)>
        {
          !UrlEncode ? (Name, Value) : (HttpUtility.UrlEncode(Name), HttpUtility.UrlEncode(Value)),
          ("Domain", Domain),
          ("Path", Path),
          ("Expires", Expires?.ToUniversalTime().ToString("r")),
          ("Max-Age", MaxAge?.ToString())
        }
        .Select(t => t.Item2 == null ? null : $"{t.Item1}={t.Item2}")
        .Concat(new List<string>
        {
          Secure ?? false ? "Secure" : null,
          HttpOnly ?? false ? "HttpOnly" : null
        }).Where(t => t != null)
        .Aggregate((a, b) => $"{a}; {b}");
    }
  }
}