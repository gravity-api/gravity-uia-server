/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Generic;
using System.Xml.Linq;

using UiaWebDriverServer.Contracts;

namespace UiaWebDriverServer.Domain.Application
{
    public interface ISessionRepository
    {
        IDictionary<string, Session> Sessions { get; }

        (int StatusCode, object Entity) CreateSession(Capabilities capabilities);
        (int StatusCode, XDocument ElementsXml) CreateSessionXml(string id);
        (int StatusCode, Session Session) GetSession(string id);
        int DeleteSession(string id);
    }
}
