/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Http;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UiaWebDriverServer.Contracts.Attributes;

namespace UiaWebDriverServer.Domain.Application
{
    public class DocumentRepository : IDocumentRepository
    {
        /// <summary>
        /// Invoke a script within the context of the current document.
        /// </summary>
        /// <param name="src">The script source code or file path.</param>
        /// <param name="type">The type of script, such as `Powershell`, `AutoIT` or other supported scripting languages.</param>
        public (int StatusCode, object Result) InvokeScript(string session, string src, string type)
        {
            return Invoke(session, src, type);
        }

        public Task InvokeScriptAsync()
        {
            throw new NotImplementedException();
        }

        private static (int StatusCode, object Result) Invoke(string session, string src, string type)
        {
            // constants
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            const StringComparison Compare = StringComparison.OrdinalIgnoreCase;

            // setup
            var methods = typeof(DocumentRepository)
                .GetMethods(Flags)
                .Where(i => i.GetCustomAttribute<ScriptTypeAttribute>() != null);
            var method = methods.FirstOrDefault(i => i.GetCustomAttribute<ScriptTypeAttribute>().Type.Equals(type, Compare));

            // not found
            if (method == default)
            {
                return (StatusCodes.Status404NotFound, string.Empty);
            }

            // invoke
            try
            {
                var result = method.Invoke(obj: null, parameters: new object[] { session, src });
                return (StatusCodes.Status200OK, result);
            }
            catch (Exception e) when (e != null)
            {
                return (StatusCodes.Status500InternalServerError, $"{e.GetBaseException().Message}");
            }
        }

        [ScriptType("Powershell")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by reflection")]
        private static object InvokePowershell(string session, string src)
        {
            // setup
            var tempPath = Path.GetTempPath();
            var fileName = $"{session}.ps1";
            var path = Path.Combine(tempPath, fileName);
            var contents = File.Exists(src) ? File.ReadAllText(src) : src;

            // new script file
            File.WriteAllText(path, contents);

            // invoke
            var startInfo = new ProcessStartInfo("powershell", $"\"{path}\"");
            var process = new Process
            {
                StartInfo = startInfo,
            };
            process.Start();
            process.WaitForExit();

            // get
            return string.Empty;
        }
    }
}
