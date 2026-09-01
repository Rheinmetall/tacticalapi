using System;
using System.Threading;

using Rheinmetall.TacticalApi.V0;

namespace C4I.Test.TacticalApi.TestClient
{
    internal static class Utilities
    {
        internal static void CheckResult(ResponseHeader responseHeader)
        {
            if (!responseHeader.Success)
            {
                throw new InvalidOperationException(responseHeader.ErrorMessage);
            }
        }

        internal static void CancelWhenCancelKeyPressed(Action<CancellationToken> getResponse)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => cts.Cancel();
            Console.WriteLine("Press CTRL+C to exit.");
            try
            {
                getResponse.Invoke(cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
            }
        }
    }
}