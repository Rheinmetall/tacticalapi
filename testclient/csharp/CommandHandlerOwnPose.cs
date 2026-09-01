using System;
using System.Globalization;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using Rheinmetall.TacticalApi.V0;

namespace C4I.Test.TacticalApi.TestClient
{
    internal class CommandHandlerOwnPose : ICommandHandler
    {
        private const string CommandSetOwnPosition = "--setownposition";
        private const string CommandPrintOwnPosition = "--printownposition";
        private const string CommandObserveOwnPosition = "--observeownposition";

        private readonly OwnPose.OwnPoseClient client;

        public CommandHandlerOwnPose(GrpcChannel channel)
        {
            this.client = new OwnPose.OwnPoseClient(channel);
        }

        public string GetUsage()
        {
            return
                $"{CommandSetOwnPosition} [lat] [lon]\n" +
                $"{CommandPrintOwnPosition}\n" +
                $"{CommandObserveOwnPosition}";
        }

        public bool TryExecuteCommand(string[] args)
        {
            if (args.Length == 3 && args[0].ToLower() == CommandSetOwnPosition
                                 && double.TryParse(args[1], CultureInfo.InvariantCulture, out var lat)
                                 && double.TryParse(args[2], CultureInfo.InvariantCulture, out var lon))
            {
                this.SetOwnPosition(lat, lon);
            }
            else if (args.Length == 1 && args[0].ToLower() == CommandPrintOwnPosition)
            {
                this.PrintOwnPosition();
            }
            else if (args.Length == 1 && args[0].ToLower() == CommandObserveOwnPosition)
            {
                this.ObserveOwnPosition();
            }
            else
            {
                return false;
            }

            return true;
        }

        private void SetOwnPosition(double latitude, double longitude)
        {
            var request = new UpdatePositionRequest
            {
                Position = new UpdatePosition
                {
                    SourceIdentifier = "Api",
                    PointLocation = new Point
                    {
                        LocationTime = Timestamp.FromDateTime(DateTime.UtcNow),
                        GeoPoint = new GeoPoint
                        {
                            LatitudeCoordinate = latitude,
                            LongitudeCoordinate = longitude,
                            MeasurementCode = MeasurementCode.Estimate
                        }
                    }
                }
            };

            Utilities.CheckResult(this.client.UpdatePosition(request).Header);
        }

        private void PrintOwnPosition()
        {
            var response = this.client.GetPosition(new GetPositionRequest());
            Utilities.CheckResult(response.Header);
            Console.WriteLine(response.Position);
        }

        private void ObserveOwnPosition()
        {
            Utilities.CancelWhenCancelKeyPressed(cancellationToken =>
            {
                var response = this.client.SubscribePositionChangedEvents(new SubscribePositionEventsRequest());
                while (response.ResponseStream.MoveNext(cancellationToken).GetAwaiter().GetResult())
                {
                    var update = response.ResponseStream.Current;
                    Utilities.CheckResult(update.Header);
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(update.Position);
                }
            });
        }
    }
}