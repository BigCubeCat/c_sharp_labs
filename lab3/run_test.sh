#!/bin/bash
dotnet clean
dotnet restore
dotnet build
dotnet test tests/Lab3.Tests/Lab3.Tests.csproj
