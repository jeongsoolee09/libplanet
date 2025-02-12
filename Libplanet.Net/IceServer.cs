using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Libplanet.Stun;
using Serilog;

namespace Libplanet.Net
{
    public class IceServer
    {
        public IceServer(
            IEnumerable<string> urls,
            string? username = null,
            string? credential = null)
            : this(urls.Select(u => new Uri(u)), username, credential)
        {
        }

        public IceServer(
            IEnumerable<Uri> urls,
            string? username = null,
            string? credential = null)
        {
            Urls = urls;
            Username = username;
            Credential = credential;
        }

        public IEnumerable<Uri> Urls { get; }

        public string? Username { get; }

        public string? Credential { get; }

        internal static async Task<TurnClient> CreateTurnClient(
            IEnumerable<IceServer> iceServers)
        {
            foreach (IceServer server in iceServers)
            {
                foreach (Uri url in server.Urls)
                {
                    if (url.Scheme != "turn")
                    {
                        throw new ArgumentException($"{url} is not a valid TURN url.");
                    }

                    int port = url.IsDefaultPort
                        ? TurnClient.TurnDefaultPort
                        : url.Port;
                    var turnClient = new TurnClient(
                        url.Host,
                        server.Username,
                        server.Credential,
                        port);

                    if (await turnClient.IsConnectable())
                    {
                        Log.Debug("TURN client created: {Host}:{Port}", url.Host, url.Port);
                        return turnClient;
                    }
                }
            }

            throw new IceServerException("Could not find a suitable ICE server.");
        }
    }
}
