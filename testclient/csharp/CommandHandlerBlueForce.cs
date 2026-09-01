using System;
using System.Globalization;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using Rheinmetall.TacticalApi.V0;

namespace C4I.Test.TacticalApi.TestClient
{
    internal class CommandHandlerBlueForce : ICommandHandler
    {
        private const string CommandUpdateBlueForce = "--updateblueforce";
        private const string CommandPrintBlueForces = "--printblueforces";
        private const string CommandObserveBlueForces = "--observeblueforces";
        private readonly BlueForceTracking.BlueForceTrackingClient client;

        public CommandHandlerBlueForce(GrpcChannel channel)
        {
            this.client = new BlueForceTracking.BlueForceTrackingClient(channel);
        }

        public string GetUsage()
        {
            return
                $"{CommandUpdateBlueForce} [id] [callsign] [lat] [lon]\n" +
                $"{CommandPrintBlueForces}\n" +
                $"{CommandObserveBlueForces}";
        }

        public bool TryExecuteCommand(string[] args)
        {
            if (args.Length == 5 && args[0].ToLower() == CommandUpdateBlueForce
                                 && double.TryParse(args[3], CultureInfo.InvariantCulture, out var lat)
                                 && double.TryParse(args[4], CultureInfo.InvariantCulture, out var lon))
            {
                this.UpdateBlueForce(args[1], args[2], lat, lon);
            }
            else if (args.Length == 1 && args[0].ToLower() == CommandPrintBlueForces)
            {
                this.PrintBlueForces();
            }
            else if (args.Length == 1 && args[0].ToLower() == CommandObserveBlueForces)
            {
                this.ObserveBlueForces();
            }
            else
            {
                return false;
            }

            return true;
        }

        private void UpdateBlueForce(string id, string callsign, double latitude, double longitude)
        {
            var blueForceTime = DateTime.UtcNow;
            var request = new AddOrUpdateBlueForcesRequest
            {
                BlueForcesToUpdates =
                {
                    new UpdateBlueForce
                    {
                        Identity = new Identity
                        {
                            StringIdentity = id
                        },
                        Callsign = callsign,
                        LastContactTime = Timestamp.FromDateTime(blueForceTime),
                        BlueForceType = new BlueForceType
                        {
                            IsUnmanned = true
                        },
                        Symbol = new SymbolIdentifier
                        {
                            SymbolCatalog = SymbolCatalog.Mil2525C,
                            StringIdentifier = "SFAPMH----****"
                        },
                        PointLocation = new Point
                        {
                            LocationTime = Timestamp.FromDateTime(blueForceTime),
                            GeoPoint = new GeoPoint
                            {
                                LatitudeCoordinate = latitude,
                                LongitudeCoordinate = longitude,
                                MeasurementCode = MeasurementCode.Gps
                            }
                        }
                    }
                }
            };

            Utilities.CheckResult(this.client.AddOrUpdateBlueForces(request).Header);
        }

        private void PrintBlueForces()
        {
            var response = this.client.GetBlueForces(new GetBlueForcesRequest());
            Utilities.CheckResult(response.Header);
            Console.WriteLine(response.BlueForces);
        }

        private void ObserveBlueForces()
        {
            Utilities.CancelWhenCancelKeyPressed(cancellationToken =>
            {
                var response = this.client.SubscribeBlueForceEvents(new SubscribeBlueForceEventsRequest());
                while (response.ResponseStream.MoveNext(cancellationToken).GetAwaiter().GetResult())
                {
                    var update = response.ResponseStream.Current;
                    Utilities.CheckResult(update.Header);
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(update.UpdatedBlueForces);
                }
            });
        }
    }
}