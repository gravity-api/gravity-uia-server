using System;
using System.Net;
using System.Net.Sockets;

namespace OpenQA.Selenium.Extensions
{
    internal static class Utilities
    {
        /// <summary>
        /// finds a random, free port to be listened on
        /// </summary>
        /// <returns>a random, free port to be listened on</returns>
        public static int FindFreePort()
        {
            int listeningPort = 0;
            var portSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                var socketEndPoint = new IPEndPoint(IPAddress.Any, 0);
                portSocket.Bind(socketEndPoint);
                socketEndPoint = (IPEndPoint)portSocket.LocalEndPoint;
                listeningPort = socketEndPoint.Port;
            }
            finally
            {
                portSocket.Close();
            }
            return listeningPort;
        }
    }
}