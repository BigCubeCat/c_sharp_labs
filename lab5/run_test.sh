#!/bin/bash
dotnet clean
dotnet restore
dotnet build
dotnet test tests/Lab.Tests/Lab.Tests.csproj
