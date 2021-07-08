/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 *    - modify: add GetSession method
 *    - modify: add GetSssionsMap method
 */
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Http;
using UiaDriverServer.Components;
using UiaDriverServer.Dto;
using UIAutomationClient;

namespace UiaDriverServer.Controllers
{
    /// <summary>
    /// base class for all web-driver controllers (holds controllers state)
    /// </summary>
    public abstract class Api : ApiController
    {
        // members: state
        internal static readonly IDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();
        internal readonly JsonSerializerSettings jsonSettings;

        protected Api()
        {
            jsonSettings = Utilities.GetJsonSettings();
        }

        /// <summary>
        /// gets a session by it's session-id
        /// </summary>
        /// <param name="id">session-id</param>
        /// <returns>session information object</returns>
        internal Session GetSession(string id)
        {
            if (!sessions.ContainsKey(id))
            {
                return null;
            }
            return sessions[id];
        }

        /// <summary>
        /// gets an element by it's element-id
        /// </summary>
        /// <param name="session">session to get the element from</param>
        /// <param name="id">element-id</param>
        /// <returns>element information object</returns>
        internal Element GetElement(Session session, string id)
        {
            if (!session.Elements.ContainsKey(id))
            {
                return null;
            }
            return session.Elements[id];
        }
    }
}