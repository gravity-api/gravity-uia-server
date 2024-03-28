/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Domain.Application;

namespace UiaWebDriverServer.Domain
{
    public class UiaDomain : IUiaDomain
    {
        public UiaDomain(
            IElementRepository elementsRepository,
            ISessionRepository sessionsRepository,
            IDocumentRepository documentRepository)
        {
            SessionsRepository = sessionsRepository;
            ElementsRepository = elementsRepository;
            DocumentRepository = documentRepository;
        }

        public ISessionRepository SessionsRepository { get; }

        public IElementRepository ElementsRepository { get; }

        public IDocumentRepository DocumentRepository { get; }

        /// <summary>
        /// Gets a session by id.
        /// </summary>
        /// <param name="id">The session id.</param>
        /// <returns>Session information object.</returns>
        public Session GetSession(string id)
        {
            if (!SessionsRepository.Sessions.ContainsKey(id) && string.IsNullOrEmpty(id))
            {
                return null;
            }
            if (!SessionsRepository.Sessions.ContainsKey(id) && !string.IsNullOrEmpty(id))
            {
                return new Session { SessionId = id };
            }
            return SessionsRepository.Sessions[id];
        }
    }
}
