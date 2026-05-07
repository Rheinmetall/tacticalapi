using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;

using Rheinmetall.TacticalApi.V0;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;
using Grpc.Net.Client.Web;

namespace C4I.Frontend.TacticalApi.TestClient
{
    /// <summary>
    /// A simple command line client for the TacticalAPI.
    /// </summary>
    internal class Program
    {
        private const string CommandObserveSituation = "--observesituation";
        private const string CommandPrintSituation = "--printsituation";
        private const string CommandSendSymbol = "--sendsymbol";
        private const string CommandChangeSymbolName = "--changesymbolname";
        private const string CommandDeleteSymbol = "--deletesymbol";

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
                var client = new Situation.SituationClient(channel);
                if (!ParseArguments(args, client))
                {
                    Console.Error.WriteLine($"Invalid arguments - usage:\n{GetUsage()}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Errors occurred:");
                Console.Error.WriteLine(ex);
            }
        }

        private static string GetUsage()
        {
            return
                $"{CommandObserveSituation}\n" +
                $"{CommandSendSymbol} [latitude] [longitude]\n" +
                $"{CommandPrintSituation}\n" +
                $"{CommandChangeSymbolName} [guid symbol identity] [name]\n" +
                $"{CommandDeleteSymbol} [guid symbol identity]";
        }

        private static bool ParseArguments(string[] args, Situation.SituationClient client)
        {
            if (args.Length == 3 && args[0].ToLower() == CommandSendSymbol
                                 && double.TryParse(args[1], CultureInfo.InvariantCulture, out var lat)
                                 && double.TryParse(args[2], CultureInfo.InvariantCulture, out var lon))
            {
                CreateSymbol(client, lat, lon);
            }
            else if (args.Length == 3 && args[0].ToLower() == CommandChangeSymbolName
                                      && Guid.TryParse(args[1], out var changeSymbolNameIdentity))
            {
                ChangeSymbolName(client, changeSymbolNameIdentity, args[2]);
            }
            else if (args.Length == 2 && args[0].ToLower() == CommandDeleteSymbol
                                      && Guid.TryParse(args[1], out var deleteSymbolIdentity))
            {
                DeleteSymbol(client, deleteSymbolIdentity);
            }
            else if (args.Length == 1 && args[0].ToLower() == CommandPrintSituation)
            {
                PrintSituation(client);
            }
            else if (args.Length == 1 && args[0].ToLower() == CommandObserveSituation)
            {
                ObserveSituation(client);
            }
            else
            {
                return false;
            }

            return true;
        }

        private static void CreateSymbol(Situation.SituationClient client, double latitude, double longitude)
        {
            var addOrUpdateSituationObjects = new AddOrUpdateSituationObjectsRequest
            {
                SituationObjects =
                {
                    new UpdateSituationObject
                    {
                        Symbol = new UpdateSymbol
                        {
                            Identity = new Identity
                            {
                                UuidIdentity = Guid.NewGuid().ToString()
                            },
                            Reporter = new Identity
                            {
                                StringIdentity = "TacticalAPI"
                            },
                            ReportingTime = Timestamp.FromDateTime(DateTime.UtcNow),
                            SymbolIdentifier = new UpdatePropertySymbolIdentifier
                            {
                                Content = new SymbolIdentifier
                                {
                                    SymbolCatalog = SymbolCatalog.Mil2525C,
                                    StringIdentifier = "SFGPEWTM--*****"
                                }
                            },
                            Location = new UpdatePropertyLocation
                            {
                                Content = new SymbolLocation
                                {
                                    Point = new Point
                                    {
                                        GeoPoint = new GeoPoint
                                        {
                                            LatitudeCoordinate = latitude,
                                            LongitudeCoordinate = longitude
                                        },
                                        LocationTime = Timestamp.FromDateTime(DateTime.UtcNow)
                                    }
                                }
                            }
                        }
                    }
                }
            };
            CheckResult(client.AddOrUpdateSituationObjects(addOrUpdateSituationObjects).Header);
        }

        private static void ChangeSymbolName(Situation.SituationClient client, Guid symbolIdentity, string symbolName)
        {
            var addOrUpdateSituationObjectsRequest = new AddOrUpdateSituationObjectsRequest
            {
                SituationObjects =
                {
                    new UpdateSituationObject
                    {
                        Symbol = new UpdateSymbol
                        {
                            Identity = new Identity
                            {
                                UuidIdentity = symbolIdentity.ToString()
                            },
                            Reporter = new Identity
                            {
                                StringIdentity = "TacticalAPI"
                            },

                            // We don't connect different systems and do the update now, so using the current time is ok here   
                            ReportingTime = Timestamp.FromDateTime(DateTime.UtcNow),

                            // For updates, you only need to supply the changed properties - all other properties are untouched
                            // To Set a property to null, set the UpdateProperty and leave its content null.   
                            Name = new UpdatePropertyString
                            {
                                Content = symbolName
                            }
                        }
                    }
                }
            };
            CheckResult(client.AddOrUpdateSituationObjects(addOrUpdateSituationObjectsRequest).Header);
        }

        private static void DeleteSymbol(Situation.SituationClient client, Guid symbolIdentity)
        {
            var deleteSituationObjectsRequest = new DeleteSituationObjectsRequest
            {
                SituationObjects =
                {
                    new DeleteSituationObject
                    {
                        Identity = new Identity
                        {
                            UuidIdentity = symbolIdentity.ToString()
                        },
                        Reporter = new Identity
                        {
                            StringIdentity = "TacticalAPI"
                        },
                        ReportingTime = Timestamp.FromDateTime(DateTime.UtcNow),
                    }
                }
            };
            CheckResult(client.DeleteSituationObjects(deleteSituationObjectsRequest).Header);
        }

        private static void PrintSituation(Situation.SituationClient client)
        {
            var response = client.GetSituationObjects(new GetSituationObjectsRequest());
            CheckResult(response.Header);
            Console.WriteLine(response.SituationObjects);
        }

        private static void ObserveSituation(Situation.SituationClient client)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => cts.Cancel();
            Console.WriteLine("Press CTRL+C to exit.");
            try
            {
                var response = client.SubscribeSituationObjectEvents(new SubscribeSituationObjectEventsRequest());

                // GetAwaiter().GetResult() is actually bad style in C# - don't use it in productive code!
                while (response.ResponseStream.MoveNext(cts.Token).GetAwaiter().GetResult())
                {
                    var update = response.ResponseStream.Current;
                    CheckResult(update.Header);
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(update.SituationObjects);
                }
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
            }
        }

        private static void CheckResult(ResponseHeader responseHeader)
        {
            if (!responseHeader.Success)
            {
                throw new InvalidOperationException(responseHeader.ErrorMessage);
            }
        }
    }
}