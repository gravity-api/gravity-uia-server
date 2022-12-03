/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Generic;

using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Domain.Application;

namespace UiaWebDriverServer.Domain
{
    public interface IUiaDomain
    {
        ISessionRepository SessionsRepository { get; }

        IElementRepository ElementsRepostiroy { get; }

        public Session GetSession(string id);
    }
}
