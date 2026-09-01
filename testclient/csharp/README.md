
# TacticalAPI Test Client

This is an example on how to use the TacticalAPI.

## Overview

The example is written in C#.
It is realized as command line application which can be used to execute common commands on the TacticalAPI. 

## Building

To build the application, you need to install the .NET SDK, version 10.0 from https://dotnet.microsoft.com
Run `dotnet build TacticalApi.TestClient.csproj` to create the executable.

## Running

To run the application, you need to install the .NET runtime, version 10.0 from https://dotnet.microsoft.com.

The different features and options for running the CLI can be viewed by executing `dotnet TacticalApi.TestClient.dll`.

## Integration
  
  - The ports used for the TacticalAPI are defined by the implementing application.
  - To connect with Rheinmetalls TacNet for example, use port 4268 for gRPC Web (HTTP/1.1) or port 4267 for the normal gRPC (HTTP/2).
