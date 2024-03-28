/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Domain.Application;

namespace UiaWebDriverServer.Domain
{
    public interface IUiaDomain
    {
        ISessionRepository SessionsRepository { get; }

        IElementRepository ElementsRepository { get; }

        IDocumentRepository DocumentRepository { get; }

        public Session GetSession(string id);
    }
}
