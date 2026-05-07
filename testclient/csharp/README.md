
# TacticalAPI Test Client

This is an example on how to use the TaticalAPI.

## Overview

The example is written in C#.
It is realized as command line application which can be used to create, read, update and delete symbols. 

## Building

- In order to build the application, you need to install the .NET SDK, version 10.0 from https://dotnet.microsoft.com
- To build the application, run `dotnet build TacticalApi.TestClient.csproj`

## Running

- In order to run the application, you need to install the .NET runtime, version 10.0 from https://dotnet.microsoft.com
- The behavior of the application can be customized with different command line arguments. Use:
  - `dotnet TacticalApi.TestClient.dll` to display the usage of the program.
  - `dotnet TacticalApi.TestClient.dll --observesituation` for an example on how to get notified about symbol changes.
  - `dotnet TacticalApi.TestClient.dll --sendsymbol 53 8.9` for an example on how to create an anti tank gun at the WGS84
    position $(53.0, 8.9)$. 
  - `dotnet TacticalApi.TestClient.dll --printsituation` to print the whole situation at a given time to the command line.
    In contrast to `--observesituation` this API is intended for polling.
  - `dotnet TacticalApi.TestClient.dll --changesymbolname 05087475-a61f-4766-bfac-177c8d3d1c5d Foo` to change the name of
    the symbol `05087475-a61f-4766-bfac-177c8d3d1c5d` to `Foo`. Note: This ID is just an example and won't work on your
    computer as a new ID is generated in every `--sendsymbol` call. Use `--printsituation` or `--observesituation` to
    get the list of valid symbol IDs.
  - `dotnet TacticalApi.TestClient.dll --deletesymbol 05087475-a61f-4766-bfac-177c8d3d1c5d` to delete the symbol
    `05087475-a61f-4766-bfac-177c8d3d1c5d`. Note: This ID is just an example and won't work on your
    computer as a new ID is generated in every `--sendsymbol` call. Use `--printsituation` or `--observesituation` to
    get the list of valid symbol IDs.

## Integration 
  
  - The ports used for the TacticalAPI are defined by the implementing application.
  - To connect with Rheinmetalls TacNet for example, use port 4268 for gRPC Web (HTTP/1.1) or port 4267 for the normal gRPC (HTTP/2).
