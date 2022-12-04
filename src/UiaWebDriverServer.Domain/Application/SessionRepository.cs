/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

using UIAutomationClient;

using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Domain.Extensions;
using UiaWebDriverServer.Extensions;

namespace UiaWebDriverServer.Domain.Application
{
    public class SessionRepository : ISessionRepository
    {
        // members
        private readonly ILogger<SessionRepository> _logger;

        public SessionRepository(IDictionary<string, Session> sessions, ILogger<SessionRepository> logger)
        {
            Sessions = sessions;
            _logger = logger;
        }

        public IDictionary<string, Session> Sessions { get; }

        public (int StatusCode, object Entity) CreateSession(Capabilities capabilities)
        {
            // setup
            var isAppCapability = capabilities.DesiredCapabilities.ContainsKey(UiaCapability.Application);
            var isApp = isAppCapability && !string.IsNullOrEmpty($"{capabilities.DesiredCapabilities[UiaCapability.Application]}");

            // create
            var (statusCode, response, seesion) = !isApp
                ? CreateDesktopSession(capabilities.DesiredCapabilities, _logger)
                : CreateApplicationSession(capabilities.DesiredCapabilities, _logger);

            // setup
            Sessions[seesion.SessionId] = seesion;

            // get
            return (statusCode, response);
        }

        public (int StatusCode, XDocument ElementsXml) CreateSessionXml(string id)
        {
            // not found
            if (!Sessions.ContainsKey(id))
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var session = Sessions[id];
            var elementsXml = DomFactory.Create(session.ApplicationRoot, TreeScope.TreeScope_Descendants);

            // get
            return (StatusCodes.Status200OK, elementsXml);
        }

        public (int StatusCode, Session Session) GetSession(string id)
        {
            // not found
            if (!Sessions.ContainsKey(id))
            {
                return (StatusCodes.Status404NotFound, default);
            }

            // setup
            var session = Sessions[id];

            // get
            return (StatusCodes.Status200OK, session);
        }

        public int DeleteSession(string id)
        {
            // not found
            if (!Sessions.ContainsKey(id))
            {
                _logger?.LogInformation("Delete-Session -Session {id} = NotFount", id);
                return StatusCodes.Status404NotFound;
            }

            // setup
            var session = Sessions[id];
            var name = session?.Application.StartInfo.FileName;

            // delete
            try
            {
                session.Application?.Kill(entireProcessTree: true);
            }
            catch (Exception e) when (e != null)
            {
                var error = e.Message;
                _logger.LogWarning("Delete-Session -Id {id} = (InternalServerError | {error})", id, error);
            }
            Sessions.Remove(id);

            // log
            const string noContent = "Delete-Session -Session {id} -Application {name} = NoContent";
            _logger?.LogInformation(noContent, id, name);

            // get
            return StatusCodes.Status204NoContent;
        }

        private static (int StatusCode, object Response, Session Seesion) CreateDesktopSession(IDictionary<string, object> capabilities, ILogger logger)
        {
            // setup
            var id = Guid.NewGuid();

            // build
            var session = new Session
            {
                SessionId = $"{id}",
                Capabilities = capabilities,
                Runtime = Array.Empty<int>(),
                TreeScope = TreeScope.TreeScope_Children,
            };

            // setup
            session.Dom = DomFactory.Create(session.ApplicationRoot, session.TreeScope);
            logger.LogInformation("Create-Session -Session {id} = Created", id);

            // build
            var response = new
            {
                Value = new
                {
                    SessionId = id,
                    Capabilities = capabilities
                }
            };

            // get
            return (StatusCodes.Status200OK, response, session);
        }

        private static (int StatusCode, object Response, Session Seesion) CreateApplicationSession(IDictionary<string, object> capabilities, ILogger logger)
        {
            // constants
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // build
            var mount = capabilities.ContainsKey(UiaCapability.Mount) && ((JsonElement)capabilities[UiaCapability.Mount]).GetBoolean();
            var executeable = $"{capabilities[UiaCapability.Application]}";
            var arguments = capabilities.TryGetValue(UiaCapability.Arguments, out object value) && value != null
                ? JsonSerializer.Deserialize<IEnumerable<string>>($"{capabilities[UiaCapability.Arguments]}")
                : Array.Empty<string>();

            // get session
            var process = mount
                ? Array.Find(Process.GetProcesses(), i => executeable.Contains(i.ProcessName, Compare))
                : DomainUtilities.StartProcess(executeable, string.Join(" ", arguments));

            // internal server error
            if (process?.MainWindowHandle == default && process.Handle == default && (process.SafeHandle.IsInvalid || process.SafeHandle.IsClosed))
            {
                return (StatusCodes.Status500InternalServerError, default, default);
            }

            // build session
            _ = capabilities.TryGetValue(UiaCapability.TreeScope, out object treeScopeOut);
            var treeScope = !string.IsNullOrEmpty($"{treeScopeOut}") && !$"{treeScopeOut}".Equals("none", Compare)
                ? $"{treeScopeOut}".ConvertToTreeScope()
                : TreeScope.TreeScope_Children;

            var session = new Session(new CUIAutomation8(), process)
            {
                Capabilities = capabilities,
                TreeScope = treeScope,
            };
            session.Dom = DomFactory.Create(session.ApplicationRoot, session.TreeScope);

            // log
            var id = session.SessionId;
            var name = session.Application.GetNameOrFile();

            logger?.LogInformation("Create-Session -Session {id} -Application {name} = Created", id, name);
            logger?.LogInformation("Get-VirtualDom = /session/{id}", id);

            // get
            var response = new
            {
                Value = new
                {
                    session.SessionId,
                    Capabilities = capabilities
                }
            };
            return (StatusCodes.Status200OK, response, session);
        }
    }
}
