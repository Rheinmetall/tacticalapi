using System;
using System.Globalization;
using System.Threading;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using Rheinmetall.TacticalApi.V0;

namespace C4I.Test.TacticalApi.TestClient
{
    internal class CommandHandlerSituation : ICommandHandler
    {
        private const string CommandObserveSituation = "--observesituation";
        private const string CommandPrintSituation = "--printsituation";
        private const string CommandSendSymbol = "--sendsymbol";
        private const string CommandChangeSymbolName = "--changesymbolname";
        private const string CommandDeleteSymbol = "--deletesymbol";
        private readonly Situation.SituationClient client;

        public CommandHandlerSituation(GrpcChannel channel)
        {
            this.client = new Situation.SituationClient(channel);
        }

        public bool TryExecuteCommand(string[] args)
        {
            if (args.Length == 3 && args[0].ToLower() == CommandSendSymbol
                                 && double.TryParse(args[1], CultureInfo.InvariantCulture, out var lat)
                                 && double.TryParse(args[2], CultureInfo.InvariantCulture, out var lon))
            {
                CreateSymbol(this.client, lat, lon);
            }
            else if (args.Length == 3 && args[0].ToLower() == CommandChangeSymbolName
                                      && Guid.TryParse(args[1], out var changeSymbolNameIdentity))
            {
                ChangeSymbolName(this.client, changeSymbolNameIdentity, args[2]);
            }
            else if (args.Length == 2 && args[0].ToLower() == CommandDeleteSymbol
                                      && Guid.TryParse(args[1], out var deleteSymbolIdentity))
            {
                DeleteSymbol(this.client, deleteSymbolIdentity);
            }
            else if (args.Length == 1 && args[0].ToLower() == CommandPrintSituation)
            {
                PrintSituation(this.client);
            }
            else if (args.Length == 1 && args[0].ToLower() == CommandObserveSituation)
            {
                ObserveSituation(this.client);
            }
            else
            {
                return false;
            }

            return true;
        }

        string ICommandHandler.GetUsage()
        {
            return
                $"{CommandObserveSituation}\n" +
                $"{CommandSendSymbol} [latitude] [longitude]\n" +
                $"{CommandPrintSituation}\n" +
                $"{CommandChangeSymbolName} [guid symbol identity] [name]\n" +
                $"{CommandDeleteSymbol} [guid symbol identity]";
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
            Utilities.CheckResult(client.AddOrUpdateSituationObjects(addOrUpdateSituationObjects).Header);
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
            Utilities.CheckResult(client.AddOrUpdateSituationObjects(addOrUpdateSituationObjectsRequest).Header);
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
            Utilities.CheckResult(client.DeleteSituationObjects(deleteSituationObjectsRequest).Header);
        }

        private static void PrintSituation(Situation.SituationClient client)
        {
            var response = client.GetSituationObjects(new GetSituationObjectsRequest());
            Utilities.CheckResult(response.Header);
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
                    Utilities.CheckResult(update.Header);
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(update.SituationObjects);
                }
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
            }
        }
    }
}