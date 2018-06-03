using Neo.Lux.Core;
using Phantasma.Bridge.Core;
using SynkServer.Core;
using SynkServer.HTTP;
using System;
using System.Threading;

namespace Phantasma.Bridge
{
    class Program
    {
        static void Main(string[] args)
        {
            // initialize a logger
            var log = new SynkServer.Core.Logger();

            // either parse the settings from the program args or initialize them manually
            var settings = ServerSettings.Parse(args);

            var server = new HTTPServer(log, settings);

            // instantiate a new site, the second argument is the file path where the public site contents will be found
            var site = new Site(server, "public");

            var api = new LocalRPCNode(10332, "http://neoscan.io");
            //var api = new RemoteRPCNode(10332, "http://neoscan.io");

            Console.WriteLine("Initializing Phantasma bridge...");
            var tx = api.GetTransaction("d56d553f38234d73d04deacd9fd5f110d572898e8bd9c62333bbf7c31e1d1658");
            var bridge = new BridgeManager(api, tx, 2313808);

            var bridgeThread = new Thread(() => {
                Console.WriteLine("Running Phantasma bridge...");
                bridge.Run();
            });
            bridgeThread.IsBackground = true;
            bridgeThread.Start();

            site.Get("/", (request) =>
            {
                return HTTPResponse.FromString("Hello world!");
            });

            Console.CancelKeyPress += delegate {
                Console.WriteLine("Stopping Phantasma bridge...");
                server.Stop();
                bridge.Stop();
                Environment.Exit(0);
            };

            server.Run();
        }
    }
}
