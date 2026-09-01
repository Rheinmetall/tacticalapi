namespace C4I.Test.TacticalApi.TestClient
{
    internal interface ICommandHandler
    {
        bool TryExecuteCommand(string[] args);

        string GetUsage();
    }
}