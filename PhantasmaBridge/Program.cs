using LunarParser;
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
            var log = new Logger();

            var settings = new ServerSettings() { environment = ServerEnvironment.Prod, host = "phantasma.io", path = ".", port = 7733 };

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
                return HTTPResponse.FromString("Phantasma Bridge API");
            });

            site.Get("/api/mailboxes", (request) =>
            {
                var root = DataNode.CreateArray("mailboxes");
                foreach (var mailbox in bridge.Mailboxes)
                {
                    var node = DataNode.CreateObject("box");
                    node.AddField("address", mailbox.address);
                    node.AddField("name", mailbox.name);
                    root.AddNode(node);
                }
                return root;
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
