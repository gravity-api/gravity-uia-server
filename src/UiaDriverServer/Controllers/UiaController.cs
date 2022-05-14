using Microsoft.AspNetCore.Mvc;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;

using UiaDriverServer.Contracts;

namespace UiaDriverServer.Controllers
{
    // TODO: migrate to DI container with mananged object and remove inherit.
    public abstract class UiaController : ControllerBase
    {
        // members: state
        internal static readonly IDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();
        internal readonly JsonSerializerOptions jsonSettings;

        protected UiaController()
        {
            jsonSettings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// gets a session by it's session-id
        /// </summary>
        /// <param name="id">session-id</param>
        /// <returns>session information object</returns>
        internal static Session GetSession(string id)
        {
            if (!sessions.ContainsKey(id) && string.IsNullOrEmpty(id))
            {
                return null;
            }
            if (!sessions.ContainsKey(id) && !string.IsNullOrEmpty(id))
            {
                return new Session { SessionId = id };
            }
            return sessions[id];
        }

        /// <summary>
        /// gets an element by it's element-id
        /// </summary>
        /// <param name="session">session to get the element from</param>
        /// <param name="id">element-id</param>
        /// <returns>element information object</returns>
        internal static Element GetElement(Session session, string id)
        {
            if (!session.Elements.ContainsKey(id))
            {
                return null;
            }
            return session.Elements[id];
        }
    }
}