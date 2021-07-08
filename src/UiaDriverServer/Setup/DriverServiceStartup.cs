/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 */
using Owin;
using System.Web.Http;
using UiaDriverServer.Components;

namespace UiaDriverServer.Setup
{
    internal class DriverServiceStartup
    {
        /// <summary>
        /// service configuration section, inject application builder to setup
        /// driver-service restful endpoint
        /// </summary>
        /// <param name="appBuilder">application builder</param>
        public void Configuration(IAppBuilder appBuilder)
        {
            // initialize configuration
            var config = new HttpConfiguration();

            // json settings
            config.Formatters.JsonFormatter.SerializerSettings = Utilities.GetJsonSettings();

            // routing settings
            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();

            // build application
            appBuilder.UseWebApi(config);
        }
    }
}