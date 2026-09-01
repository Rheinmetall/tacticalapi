using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Grpc.Net.Client;
using Grpc.Net.Client.Web;

namespace C4I.Test.TacticalApi.TestClient
{
    /// <summary>
    /// A simple command line client for the TacNetApi.
    /// </summary>
    internal class Program
    {
        private static IEnumerable<ICommandHandler> GetCommandHandlers(GrpcChannel channel)
        {
            yield return new CommandHandlerSituation(channel);
            yield return new CommandHandlerBlueForce(channel);
            yield return new CommandHandlerOwnPose(channel);
        }

        public static void Main(string[] args)
        {
            try
            {
                var webHandler = new GrpcWebHandler(new HttpClientHandler());
                var channel = GrpcChannel.ForAddress("http://localhost:4268", new GrpcChannelOptions
                {
                    HttpHandler = webHandler,
                    HttpVersion = new Version(1, 1)
                });
                var commandHandlers = GetCommandHandlers(channel).ToList();
                var commandParsed = commandHandlers.Select(h => h.TryExecuteCommand(args)).Any(b => b);
                if (!commandParsed)
                {
                    var usage = string.Join("\n", commandHandlers.Select(h => h.GetUsage()));
                    Console.Error.WriteLine($"Invalid arguments - usage:\n{usage}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Errors occurred:");
                Console.Error.WriteLine(ex);
            }
        }
    }
}