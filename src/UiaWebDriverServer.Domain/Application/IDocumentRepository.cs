/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Threading.Tasks;

namespace UiaWebDriverServer.Domain.Application
{
    public interface IDocumentRepository
    {
        /// <summary>
        /// Invoke a script within the context of the current document.
        /// </summary>
        /// <param name="src">The script source code or file path.</param>
        /// <param name="type">The type of script, such as `Powershell`, `AutoIT` or other supported scripting languages.</param>
        (int StatusCode, object Result) InvokeScript(string session, string src, string type);
        Task InvokeScriptAsync();
    }
}
