﻿/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.Domain.Application;

namespace UiaWebDriverServer.Domain
{
    public class UiaDomain : IUiaDomain
    {
        public UiaDomain(
            IElementRepository elementsRepository,
            ISessionRepository sessionsRepository)
        {
            SessionsRepository = sessionsRepository;
            ElementsRepostiroy = elementsRepository;
        }

        public ISessionRepository SessionsRepository { get; }

        public IElementRepository ElementsRepostiroy { get; }

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